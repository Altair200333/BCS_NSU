import numpy as np

# nomal gaussian elimination for diagonal matrix
def gauss_illimination(elliminated, right_side):
    N_x = right_side.shape[0]
    next_vector = np.zeros(N_x)

    for i in range(N_x):
        next_vector[N_x - 1 - i] = right_side[N_x - 1 - i] - np.dot(next_vector, elliminated[N_x - 1 - i, :])
        next_vector[N_x - 1 - i] = next_vector[N_x - 1 - i] / elliminated[N_x - 1 - i, N_x - 1 - i]

    return next_vector


# Crank-Nicolson matrix ellimination
# A is already a tridiagonal matrix so we can iterate row by row
def elliminate_crank(A):
    A = A.copy()
    N_x = A.shape[0]
    ratios = []
    for i in range(N_x - 2):
        ratio = A[i + 1, i] / A[i, i]
        ratios.append(ratio)
        A[i + 1, :] = A[i + 1, :] - A[i, :] * ratio

    return A, ratios
