using System;

namespace MedicalLabAnalyzer.Helpers
{
    /// <summary>
    /// Kuhn-Munkres / Hungarian algorithm for optimal assignment (minimum cost).
    /// Input: costMatrix (n x m) as double[,] - algorithm expects square matrix: we'll pad if needed.
    /// </summary>
    public static class HungarianAlgorithm
    {
        public static int[] Solve(double[,] cost)
        {
            int n = Math.Max(cost.GetLength(0), cost.GetLength(1));
            double[,] a = new double[n, n];
            
            // Pad the matrix to make it square
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    a[i, j] = (i < cost.GetLength(0) && j < cost.GetLength(1)) ? cost[i, j] : 1e9;

            int[] u = new int[n + 1];
            int[] v = new int[n + 1];
            int[] p = new int[n + 1];
            int[] way = new int[n + 1];
            double[] minv = new double[n + 1];
            bool[] used = new bool[n + 1];

            int[] ans = new int[cost.GetLength(0)];
            
            for (int i = 1; i <= n; ++i)
            {
                p[0] = i;
                Array.Fill(minv, double.PositiveInfinity);
                Array.Fill(used, false);
                int j0 = 0;
                
                do
                {
                    used[j0] = true;
                    int i0 = p[j0], j1 = 0;
                    double delta = double.PositiveInfinity;
                    
                    for (int j = 1; j <= n; ++j)
                    {
                        if (used[j]) continue;
                        double cur = a[i0 - 1, j - 1] - u[i0] - v[j];
                        if (cur < minv[j]) 
                        { 
                            minv[j] = cur; 
                            way[j] = j0; 
                        }
                        if (minv[j] < delta) 
                        { 
                            delta = minv[j]; 
                            j1 = j; 
                        }
                    }
                    
                    for (int j = 0; j <= n; ++j)
                    {
                        if (used[j]) 
                        { 
                            u[p[j]] += (int)delta; 
                            v[j] -= (int)delta; 
                        }
                        else 
                            minv[j] -= delta;
                    }
                    j0 = j1;
                } while (p[j0] != 0);
                
                do
                {
                    int j1 = way[j0];
                    p[j0] = p[j1];
                    j0 = j1;
                } while (j0 != 0);
            }
            
            int[] assignment = new int[n];
            for (int j = 1; j <= n; ++j)
            {
                if (p[j] > 0 && p[j] <= n && j - 1 < cost.GetLength(1) && p[j] - 1 < cost.GetLength(0))
                    assignment[p[j] - 1] = j - 1;
            }
            
            return assignment;
        }
    }
}