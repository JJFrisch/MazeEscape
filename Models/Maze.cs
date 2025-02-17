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
    public class MazeModel
    {
        public List<List<MazeCell>> Cells { get; set; }
        public List<(int, int)> Path {  get; set; }
        public int PathLength { get; set; }

        public (int, int) Start { get; set; }
        public (int, int) End { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public PlayerModel Player = new PlayerModel(0,0);

        public System.Random rnd = new System.Random();

        // Types so far: GenerateBacktracking, GenerateHuntAndKill
        public List<string> MazeTypes = new List<string> { "GenerateBacktracking", "GenerateHuntAndKill", "GeneratePrims", 
            "GenerateGrowingTree_50_50", "GenerateGrowingTree_75_25", "GenerateGrowingTree_25_75", "GenerateGrowingTree_50_0", 
            "GenerateKruskals"};
        public delegate void MazeGenerationDelegate (int width, int height);
        public Dictionary<string, MazeGenerationDelegate> MazeGenerationDelegateList;



        public MazeModel()
        {
            Cells = new List<List<MazeCell>>();
            Path = new List<(int, int)> ();

            MazeGenerationDelegateList = new Dictionary<string, MazeGenerationDelegate> ();
            MazeGenerationDelegateList.Add("GenerateBacktracking", GenerateBacktracking);
            MazeGenerationDelegateList.Add("GenerateHuntAndKill", GenerateHuntAndKill);
            MazeGenerationDelegateList.Add("GeneratePrims", GeneratePrims);
            MazeGenerationDelegateList.Add("GenerateGrowingTree_50_50", GenerateGrowingTree_50_50);
            MazeGenerationDelegateList.Add("GenerateGrowingTree_75_25", GenerateGrowingTree_75_25);
            MazeGenerationDelegateList.Add("GenerateGrowingTree_25_75", GenerateGrowingTree_25_75);
            MazeGenerationDelegateList.Add("GenerateGrowingTree_50_0", GenerateGrowingTree_50_0);
            MazeGenerationDelegateList.Add("GenerateKruskals", GenerateKruskals);


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

        public List<(int, int)> WalledOffNeighbors((int, int) cell)
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
            for (int i = 0; i < 4; i++)
            {
                (int, int) new_cell = directions[i](cell);

                if (InBounds(new_cell) && !acessable[i])
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

            PathLength = max_len;
            Path = path[max_key];
        }

        public List<(int, int)> FindPathFrom() // Used for the hint power-up. Finds path from current position to the finish and returns the path.
        {
            Dictionary<(int, int), bool> visited = new Dictionary<(int, int), bool>();
            for (var col = 0; col < Height; col++)
            {
                for (var row = 0; row < Width; row++)
                {
                    visited[(row, col)] = false;
                }
            }
            var (x, y) = (Player.X, Player.Y);
            visited[(x,y)] = true;

            var path = SearchForFinish(visited, (x,y));
            return path;
        }

        public List<(int, int)> SearchForFinish(Dictionary<(int, int), bool> visited, (int,int) cell)
        {
            var neighbors = Neighbors(cell);
            foreach (var n in neighbors)
            {
                if (visited[n]) continue;
                visited[n] = true;

                if (n == End)
                {
                    return new List<(int, int)> { n, cell };
                }
                List<(int, int)> x = SearchForFinish(visited, n);
                if (x.Count > 0)
                {
                    x.Add(cell);
                    return x;
                }
            }
            return new List<(int, int)>() { };
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

        public void GeneratePrims(int width, int height)
        {
            Cells = InitializeMaze(width, height, 0);
            Width = width;
            Height = height;

            (int, int) current_cell = RandomStart();

            List<(int, int, int, int)> cellPairsList = new List<(int, int, int, int)>(); // x1, y1, x2, y2. (cell 1 is the base cell)
            Dictionary<(int, int), bool> visited = new Dictionary<(int, int), bool>();
            for (var col = 0; col < Height; col++)
            {
                for (var row = 0; row < Width; row++)
                {
                    visited[(row, col)] = false;
                }
            }
            visited[current_cell] = true;

            List<(int, int)> neighbores = AdjacentNotVisited(visited, current_cell);
            foreach (var cell in neighbores)
            {
                cellPairsList.Add((current_cell.Item1, current_cell.Item2, cell.Item1, cell.Item2));
            }
            while (cellPairsList.Count > 0)
            {
                int index = rnd.Next(cellPairsList.Count);
                var current_pair = cellPairsList[index];
                if (!visited[(current_pair.Item3, current_pair.Item4)])
                {
                    LinkCells((current_pair.Item1, current_pair.Item2), (current_pair.Item3, current_pair.Item4));
                    visited[(current_pair.Item3, current_pair.Item4)] = true;
                    foreach (var cell in AdjacentNotVisited(visited, (current_pair.Item3, current_pair.Item4)))
                    {

                            cellPairsList.Add((current_pair.Item3, current_pair.Item4, cell.Item1, cell.Item2));
                        
                    }
                }
                cellPairsList.Remove(current_pair);
            }

            OptimizeMaze();
        }

        public void GenerateGrowingTree_50_50(int width, int height)
        {
            GenerateGrowingTree(width, height, 50, 50);
        }
        public void GenerateGrowingTree_75_25(int width, int height)
        {
            GenerateGrowingTree(width, height, 75, 25);
        }
        public void GenerateGrowingTree_25_75(int width, int height)
        {
            GenerateGrowingTree(width, height, 25, 75);
        }
        public void GenerateGrowingTree_50_0(int width, int height)
        {
            GenerateGrowingTree(width, height, 50, 0);
        }

        public void GenerateGrowingTree(int width, int height, int recursive_percent, int prims_percent)
        {
            Cells = InitializeMaze(width, height, 0);
            Width = width;
            Height = height;

            (int, int) current_cell = RandomStart();

            List<(int, int)> cellList = new List<(int, int)>(); 
            cellList.Add(current_cell);

            Dictionary<(int, int), bool> visited = new Dictionary<(int, int), bool>();
            for (var col = 0; col < Height; col++)
            {
                for (var row = 0; row < Width; row++)
                {
                    visited[(row, col)] = false;
                }
            }
            visited[current_cell] = true;


            List<(int, int)> neighbores = new List<(int, int)>();
            (int, int) next_cell = (0, 0);

            while (cellList.Count > 0)
            {
                int chance = rnd.Next(0, 100);

                if (chance <= prims_percent) // Do Choose Random (Prims)
                {
                    current_cell = cellList[rnd.Next(cellList.Count)];
                }
                else if (chance <= recursive_percent + prims_percent) // Do Choose Last Entered (Recursive Backtracking)
                {
                    current_cell = cellList[cellList.Count - 1];
                }
                else // Do Choose Oldest (Mkaes the maze easier but looks cool)
                {
                    current_cell = cellList[0];
                }


                neighbores = AdjacentNotVisited(visited, current_cell);
                if (neighbores.Count == 0)
                {
                    cellList.Remove(current_cell);
                }
                else
                {
                    next_cell = neighbores[rnd.Next(neighbores.Count)];
                    LinkCells(current_cell, next_cell);
                    cellList.Add(next_cell);
                    visited[next_cell] = true;
                }
            }

            OptimizeMaze();
        }

        public void GenerateKruskals(int width, int height)
        {
            Dictionary<(int, int), List<(int, int)>> sets = new Dictionary<(int, int), List<(int, int)>>();
            List<(int, int)> cellList = new List<(int, int)>();

            for (int row = 0; row < height; row++)
            {
                List<MazeCell> cellRow = new List<MazeCell>();
                for (int col = 0; col < width; col++)
                {
                    cellRow.Add(new MazeCell(col, row, 0));
                    sets[(col, row)] = new List<(int, int)>() { };
                    cellList.Add((col, row));
                }
                Cells.Add(cellRow);
            }


            Width = width;
            Height = height;

            (int, int) current_cell = RandomStart();
            int num_walls_down = 0;

            while (num_walls_down < Width*Height - 1)
            {
                current_cell = cellList[rnd.Next(cellList.Count)];
                var choices = WalledOffNeighbors(current_cell);
                while (choices.Count > 0) 
                {
                    var other_cell = choices[rnd.Next(choices.Count)];
                    if (!sets[other_cell].Contains(current_cell))
                    {
                        LinkCells(other_cell, current_cell);
                        num_walls_down++;
                        // Add to each others sets and all they connect to
                        foreach (var cell in sets[other_cell])
                        {
                            sets[cell].Add(current_cell);
                            sets[cell].AddRange(sets[current_cell]);
                        }
                        foreach (var cell in sets[current_cell])
                        {
                            sets[cell].Add(other_cell);
                            sets[cell].AddRange(sets[other_cell]);
                        }
                        sets[current_cell].AddRange(sets[other_cell]);
                        sets[other_cell].AddRange(sets[current_cell]);

                        sets[current_cell].Add(other_cell);
                        sets[other_cell].Add(current_cell);


                        choices.Remove(other_cell);
                        break;
                    }

                    choices.Remove(other_cell);
                }

                if (choices.Count == 0)
                {
                    cellList.Remove(current_cell);
                }
            }

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
