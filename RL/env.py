import pdb
import numpy as np
from typing import Any, Dict, List, Tuple, Union

import gym
from gym import spaces

from gym_unity.envs.vectorized_env import UnityGymException

from mlagents_envs import logging_util
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.base_env import DecisionSteps, TerminalSteps, BehaviorSpec

logger = logging_util.get_logger(__name__)
logging_util.set_log_level(logging_util.INFO)

Observation = Tuple[np.ndarray, np.ndarray]
BehaviorResult = Tuple[Observation, np.ndarray, np.ndarray, Dict]
GymStepResult = Dict[str, BehaviorResult]


class MultiAgentEnv(gym.Env):
    def __init__(self, unity_env: UnityEnvironment, uint8_visual=False):
        self._env = unity_env
        self._uint8_visual = uint8_visual

        if not self._env.behavior_specs:
            self._env.step()
        self._behaviors = dict(self._env.behavior_specs)
        self._active_agents: Dict[str, Any] = dict()
        self._env.reset()
        self._assert_observations_exist()
        self._check_visual_observations()
        self._initialize_action_spaces()
        self._initialize_observation_spaces()

    def reset(self) -> Observation:
        self._env.reset()
        res = dict()
        for behavior_name in self._behaviors:
            decision_step, _ = self._env.get_steps(behavior_name)
            res[behavior_name] = self._single_step(decision_step)[0]
        return res

    def step(self, actions: Dict[str, np.ndarray]) -> GymStepResult:
        obs, reward, done, info = {}, {}, {}, {}
        for behavior_name in self._behaviors:
            for agent_id, action in actions[behavior_name].items():
                self._env.set_action_for_agent(behavior_name, agent_id, action)
        self._env.step()
        for behavior_name in self._behaviors:
            decision_step, terminal_step = self._env.get_steps(behavior_name)
            (
                obs[behavior_name],
                reward[behavior_name],
                done[behavior_name],
                info[behavior_name],
            ) = self._single_step(decision_step)
        return (obs, reward, done, info)

    def _single_step(self, info: Union[DecisionSteps, TerminalSteps]) -> BehaviorResult:
        default_observation = []
        for obs in info.obs:
            if len(obs.shape) == 4:
                default_observation.append(self._preprocess_single(obs))
            else:
                default_observation.append(obs)
        done = np.array([False for _ in info.reward])
        return (
            (default_observation, info.agent_id),
            info.reward,
            done,
            {"step": info},
        )

    def _preprocess_single(self, single_visual_obs: np.ndarray) -> np.ndarray:
        if self._uint8_visual:
            return (255.0 * single_visual_obs).astype(np.uint8)
        else:
            return single_visual_obs

    def _initialize_action_spaces(self):
        action_space = dict()
        for behavior_name, spec in self._behaviors.items():
            decision_steps, _ = self._env.get_steps(behavior_name)
            high = np.ones(spec.action_shape, dtype=np.float32)
            action_space[behavior_name] = spaces.Dict(
                {
                    agent_id: spaces.Box(-high, high)
                    for agent_id in decision_steps.agent_id
                }
            )
        self._action_space = spaces.Dict(action_space)

    def _initialize_observation_spaces(self):
        observation_space = dict()
        for behavior_name, spec in self._behaviors.items():
            decision_steps, _ = self._env.get_steps(behavior_name)
            obs_space = []
            for shape in spec.observation_shapes:
                high = np.ones(shape, dtype=np.float32)
                obs_space.append(spaces.Box(-high, high))
            obs_space = obs_space[0] if len(obs_space) == 1 else spaces.Tuple(obs_space)
            observation_space[behavior_name] = spaces.Dict(
                {agent_id: obs_space for agent_id in decision_steps.agent_id}
            )
        self._observation_space = spaces.Dict(observation_space)

    def _assert_observations_exist(self):
        for behavior_name, spec in self._behaviors.items():
            if self._get_n_vector_obs == 0 and self._get_n_visual_obs == 0:
                raise UnityGymException(
                    "There are no observations provided by the environment"
                    f" for the behaviour {behavior_name}."
                )

    def _check_visual_observations(self):
        contains_visuals = any(
            [self._get_n_visual_obs(spec) for spec in self._behaviors.values()]
        )
        if not contains_visuals and self._uint8_visual:
            logger.warning(
                "uint8_visual was set to true, "
                "but visual observations are not in use."
                "This setting will not have any effect."
            )

    def _get_n_visual_obs(self, spec: BehaviorSpec) -> int:
        return sum([1 for shape in spec.observation_shapes if len(shape) == 3])

    def _get_n_vector_obs(self, spec: BehaviorSpec) -> int:
        return sum([1 for shape in spec.observation_shapes if len(shape) == 1])

    def close(self):
        self._env.close()

    def render(self):
        raise NotImplementedError

    @property
    def action_space(self):
        return self._action_space

    @property
    def observation_space(self):
        return self._observation_space
