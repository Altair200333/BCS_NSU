import numpy as np
import scipy as sp
import matplotlib.pyplot as plt
import math
from gauss import *


def create_right_side(U, a, N_x, t):
    right_side = [(a * U[i, t] + 2 * (1 - a) * U[i + 1, t] + a * U[i + 2, t]) for i in range(0, N_x - 2)]
    right_side.append(0)

    return np.insert(right_side, 0, 0)


def compute_next_time(U, a, N_x, eliminated, ratios, t):
    right_side = create_right_side(U, a, N_x, t)

    #repeat elimination for right side of equation
    for i in range(N_x - 2):
        right_side[i + 1] = right_side[i + 1] - right_side[i] * ratios[i]

    #basically solve A*U = b
    return gauss_illimination(eliminated, right_side)


def fill_map(U, a, N_x, N_t, elliminated, ratios):
    result = U.copy()
    
    for i in range(N_t - 1):
        result[:, i + 1] = compute_next_time(result, a, N_x, elliminated, ratios, i)

    return result

#More info:
#page 195 of:
#https://www.math.hkust.edu.hk/~machas/numerical-methods-for-engineers.pdf

#https://www.youtube.com/watch?v=f_JZRjt8AZ4&ab_channel=JeffreyChasnov
def solve_crank_nicolson(U, D, L, T, N_x, N_t):
    h = L / (N_x - 1)
    dt = T / (N_t - 1)

    a = dt * D / (h ** 2)

    #build diagonal matrix
    A = np.diagflat(np.ones(N_x) * 2 * (1 + a)) + \
        np.diagflat(np.ones(N_x - 1) * (-a), 1) + \
        np.diagflat(np.ones(N_x - 1) * (-a), -1)

    A[0, 0] = A[-1, -1] = 1
    A[0, 1] = A[-1, -2] = 0
    
    #gaussian ellimination and it's steps
    eliminated, ratios = elliminate_crank(A)

    res = fill_map(U, a, N_x, N_t, eliminated, ratios)

    return res
