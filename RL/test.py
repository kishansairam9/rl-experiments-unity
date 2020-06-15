import pdb
import cv2
import torch
import numpy as np
from torchvision.utils import make_grid

from mlagents_envs.environment import UnityEnvironment
from gym_unity.envs import UnityToGymVectorized

def save_obs(obs: np.ndarray, i: int=0, ret_frame: bool=False) -> bool:
    imggrid = make_grid(torch.tensor(obs).permute(0, 3, 1, 2), nrow=5).numpy()
    imggrid = imggrid.transpose(1, 2, 0)
    if ret_frame:
        return imggrid
    return cv2.imwrite(f'ReadmeAssets/obs_{i}.png', imggrid)

def main():
    uenv = UnityEnvironment(seed=np.random.randint(65535))
    env = UnityToGymVectorized(uenv, uint8_visual=True, allow_multiple_obs=True)
    img_obs, vec_obs = env.reset()
    frames = [save_obs(np.array(img_obs[..., ::-1]))]

    for i in range(200):
        actions = np.stack([env.action_space.sample() for _ in vec_obs])
        (img_obs, vec_obs), r, d, _ = env.step(actions)
        # pdb.set_trace()
        # print(img_obs.shape)
        frames.append(save_obs(np.array(img_obs[..., ::-1]), i+1))
    pdb.set_trace()

if __name__ == "__main__":
    main()
