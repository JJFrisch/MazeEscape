using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MazeEscape.Models
{
    public class Maze
    {
        public List<List<MazeCell>> Cells { get; set; }
        public List<(int, int)> Path {  get; set; }

        public (int, int) Start { get; set; }
        public (int, int) End { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public PlayerModel Player = new PlayerModel(0,0);

        public System.Random rnd = new System.Random();
        public Maze()
        {
            Cells = new List<List<MazeCell>>();
            Path = new List<(int, int)> ();
        }

        public (int,int) RandomStart()
        {
            return (rnd.Next(0, Width - 1), rnd.Next(0, Height - 1));
        }

        public void GenerateHuntAndKill(int width, int height)
        {
            Cells = InitializeMaze(width, height, 0);
            Width = width;
            Height = height;

            (int, int) current_cell = RandomStart();

            Dictionary<(int, int), bool> visited = new Dictionary<(int, int), bool>();
            for (var col = 0; col < Height; col++)
            {
                for (var row = 0; row < Width; row++)
                {
                    visited[(row, col)] = false;
                }
            }

            int unvisited_count = Width * Height;

            Cells[current_cell.Item2][current_cell.Item1].Value = 2;
            Start = (current_cell.Item1, current_cell.Item2);
            Player.X = current_cell.Item1;
            Player.Y = current_cell.Item2;

            visited[current_cell] = true;
            unvisited_count -= 1;

            while (unvisited_count > 0)
            {
                List<(int,int)> moves = AdjacentNotVisited(visited, current_cell);
                (int, int) new_cell;

                if (moves.Count > 0)
                {
                    new_cell = moves[rnd.Next(moves.Count)];
                }
                else
                {
                    (current_cell, new_cell) = Hunt(visited);
                }

                LinkCells(current_cell, new_cell);   

                visited[new_cell] = true;
                unvisited_count--;

                current_cell = new_cell;
            }

            OptimizeMaze();
        }

        public void LinkCells((int,int) cell1, (int,int) cell2)
        {
            if (cell1.Item1 == cell2.Item1)
            {
                Cells[Math.Max(cell1.Item2, cell2.Item2)][cell1.Item1].North = false;
            }
            else if (cell1.Item2 == cell2.Item2)
            {
                Cells[cell1.Item2][Math.Min(cell1.Item1, cell2.Item1)].East = false;
            }
            else
            {
                throw new Exception("AHH LinkCells func doesn't work");
            }
        }


        public delegate (int,int) MethodDelegate((int,int) cell);
        public List<(int,int)> AdjacentNotVisited(Dictionary<(int, int), bool> visited, (int,int) cell)
        {
            List<(int, int)> moves = new List<(int, int)>();

            var directions = new List<MethodDelegate> { North, South, East, West };
            foreach (MethodDelegate dir in directions)
            {
                (int, int) new_cell = dir(cell);

                if (InBounds(new_cell))
                {
                    if (!visited[new_cell])
                    {
                        moves.Add(new_cell);
                    }
                }
            }
            return moves;
        }

        public List<(int, int)> AdjacentVisited(Dictionary<(int, int), bool> visited, (int, int) cell)
        {
            List<(int, int)> moves = new List<(int, int)>();

            var directions = new List<MethodDelegate> { North, South, East, West };
            foreach (MethodDelegate dir in directions)
            {
                (int, int) new_cell = dir(cell);

                if (InBounds(new_cell))
                {
                    if (visited[new_cell])
                    {
                        moves.Add(new_cell);
                    }
                }
            }
            return moves;
        }

        public ((int,int), (int,int)) Hunt(Dictionary<(int, int), bool> visited)
        {
            for (var col = 0; col < Height; col++)
            {
                for (var row = 0; row < Width; row++)
                {
                    if (visited[(row, col)]) continue;
                    
                    List<(int, int)> adjacent = AdjacentVisited(visited, (row,col));

                    if (adjacent.Count == 0) continue;

                    System.Random rnd = new System.Random();
                    (int, int) new_cell = adjacent[rnd.Next(0, adjacent.Count - 1)];
                    return (new_cell, (row,col));

                }
            }
            throw new Exception("AHH Hunt func doesn't work");
        }

        public bool InBounds((int,int) cell)
        {
            if (0 > cell.Item1 || cell.Item1 >= Cells[0].Count)
            {
                return false;
            }
            if (0 > cell.Item2 || cell.Item2 >= Cells.Count)
            {
                return false;
            }
            return true;
        }


        public List<List<MazeCell>> InitializeMaze(int width, int height, int value)
        {
            List<List<MazeCell>> lst = new List<List<MazeCell>>();
            for (int row = 0; row < height; row++)
            {
                List<MazeCell> cellRow = new List<MazeCell>();
                for (int col = 0; col < width; col++)
                {
                    cellRow.Add(new MazeCell(col, row, value));
                }
                lst.Add(cellRow);
            }

            return lst;
        }

        public void AddRowByList(List<int> values)
        {
            List < MazeCell > row = new List<MazeCell >();
            foreach (int value in values)
            {
                row.Add(new MazeCell (value));
            }
            Cells.Add(row);
        }

        public (int, int) North((int, int) pos)
        {
            return (pos.Item1, pos.Item2 - 1);
        }
        public (int, int) South((int, int) pos)
        {
            return (pos.Item1, pos.Item2 + 1);
        }
        public (int, int) East((int, int) pos)
        {
            return (pos.Item1 + 1, pos.Item2);
        }
        public (int, int) West((int, int) pos)
        {
            return (pos.Item1 - 1, pos.Item2);
        }

        public List<(int, int)> Neighbors((int, int) cell)
        {
            List<(int, int)> neighbors = new List<(int, int)>();
            var acessable = new List<bool> { !Cells[cell.Item2][cell.Item1].North };
            if (cell.Item2 + 1 < Height)
            {
                acessable.Add(!Cells[cell.Item2 + 1][cell.Item1].North);
            }
            else
            {
                acessable.Add(false);
            }
            acessable.Add(!Cells[cell.Item2][cell.Item1].East);
            if (cell.Item1 - 1 >= 0)
            {
                acessable.Add(!Cells[cell.Item2][cell.Item1 - 1].East);
            }
            else
            {
                acessable.Add(false);
            }
            var directions = new List<MethodDelegate> { North, South, East, West };
            for (int i=0;i<4;i++)
            {
                (int, int) new_cell = directions[i](cell);

                if (InBounds(new_cell) && acessable[i])
                {
                    neighbors.Add(new_cell);
                }
            }
            return neighbors;
        }

        public void OptimizeMaze()
        {
            Queue<(int,int)> myQueue = new Queue< (int, int) > ();
            myQueue.Enqueue(Start);
            Dictionary<(int, int), (int, int)?> came_from = new Dictionary<(int, int), (int, int)?>();
            Dictionary<(int, int), List<(int, int)>> path = new Dictionary<(int, int), List<(int, int)>>();
            came_from[Start] = null;
            path[Start] = new List<(int, int)> { Start };

            while (myQueue.Count != 0)
            {
                (int, int) current_cell = myQueue.Dequeue();

                foreach ((int,int) next_cell in Neighbors(current_cell))
                {

                    if (!came_from.ContainsKey(next_cell))
                    {
                        path[next_cell] = [.. path[current_cell], current_cell];
                        myQueue.Enqueue(next_cell);
                        came_from[next_cell] = current_cell;
                    }
                }
            }

            (int, int) max_key = Start;
            int max_len = 0;
            foreach ((int, int) key in path.Keys)
            {
                int current_len = path[key].Count;
                if (current_len > max_len)
                {
                    max_key = key;
                    max_len = current_len;
                }
            }

            Cells[End.Item2][End.Item1].Value = 0;
            End = (max_key.Item1, max_key.Item2);
            Cells[max_key.Item2][max_key.Item1].Value = 3;

            Path = path[max_key];
        }

        public void MakePathRecursive(Dictionary<(int, int), bool> visited, (int,int) cell)
        {
            var neighbors = AdjacentNotVisited(visited,cell).OrderBy(_ => rnd.Next()).ToList();
            foreach (var n in neighbors)
            {
                if (visited[n]) continue;
                visited[n] = true;
                LinkCells(cell, n);
                MakePathRecursive(visited, n);
            }
        }

        public void GenerateBacktracking(int width, int height)
        {
            Cells = InitializeMaze(width, height, 0);
            Width = width;
            Height = height;

            (int, int) current_cell = RandomStart();

            Dictionary<(int, int), bool> visited = new Dictionary<(int, int), bool>();
            for (var col = 0; col < Height; col++)
            {
                for (var row = 0; row < Width; row++)
                {
                    visited[(row, col)] = false;
                }
            }

            Cells[current_cell.Item2][current_cell.Item1].Value = 2;
            Start = (current_cell.Item1, current_cell.Item2);
            Player.X = current_cell.Item1;
            Player.Y = current_cell.Item2;

            visited[current_cell] = true;

            MakePathRecursive(visited, current_cell);
            OptimizeMaze();

        }
    }


    public class PlayerModel
    {
        public int X { get; set; }
        public int Y { get; set; }

        public PlayerModel(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
