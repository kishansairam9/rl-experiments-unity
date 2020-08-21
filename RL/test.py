import pdb
import cv2
import torch
import numpy as np
from torchvision.utils import make_grid

from mlagents_envs.environment import UnityEnvironment
from gym_unity.envs import UnityToGymVectorized
from mlagents_multiagent.mlagents_multiagent.multiagent_environment import MultiAgentEnv
from mlagents_multiagent.mlagents_multiagent.memory import Episode
from mlagents_envs.side_channel.engine_configuration_channel import (
    EngineConfigurationChannel,
)


def save_obs(obs: np.ndarray, i: int = 0, ret_frame: bool = False) -> bool:
    imggrid = make_grid(torch.tensor(obs).permute(0, 3, 1, 2), nrow=5).numpy()
    imggrid = imggrid.transpose(1, 2, 0)
    if ret_frame:
        return imggrid
    return cv2.imwrite(f"Logs/imgs/obs_{i}.png", imggrid)


def main():
    channel = EngineConfigurationChannel()
    seed = np.random.randint(65535)
    # seed = 61504

    print(seed)
    uenv = UnityEnvironment(seed=seed, side_channels=[channel])
    channel.set_configuration_parameters(time_scale=2)
    env = MultiAgentEnv(uenv, uint8_visual=True)
    eps = Episode()
    # env = UnityToGymVectorized(uenv, uint8_visual=True, allow_multiple_obs=False)
    obs = env.reset()
    eps.add(obs["MinionG2G?team=0"][0])
    # pdb.set_trace()
    # frames = [save_obs(np.array(obs["VisualPyramids?team=0"][0][..., ::-1]))]

    for i in range(1200):
        # samples = env.action_space.sample()
        print(obs["MinionG2G?team=0"][0][1])
        # x, y = map(float, input().strip().split(" "))
        x, y = 1, 1
        actions = {x: {} for x in obs}
        for behavior in obs:
            for agent_id in obs[behavior][1]:
                actions[behavior][agent_id] = np.array([x, y])
                # actions[behavior][agent_id] = samples[behavior][agent_id]
        obs, r, d, _ = env.step(actions)
        eps.add(obs["MinionG2G?team=0"][0], np.array([x, y]), r, d)
        # pdb.set_trace()
        # pdb.set_trace()
        # pdb.set_trace()
        # print(r)
        # frames.append(
        #     save_obs(np.array(obs["VisualPyramids?team=0"][0][..., ::-1]), i + 1)
        # )
        print("-" * 40)
    pdb.set_trace()


if __name__ == "__main__":
    main()
