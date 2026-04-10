using System.Collections.Generic;
using System.Linq;

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
            "GenerateKruskals", "GenerateWilsons", "GenerateAldousBroder", "GenerateBinaryTree",
            "GenerateSidewinder", "GenerateEllers", "GenerateRecursiveDivision", "GenerateSpiralBacktracker"};
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
            MazeGenerationDelegateList.Add("GenerateWilsons", GenerateWilsons);
            MazeGenerationDelegateList.Add("GenerateAldousBroder", GenerateAldousBroder);
            MazeGenerationDelegateList.Add("GenerateBinaryTree", GenerateBinaryTree);
            MazeGenerationDelegateList.Add("GenerateSidewinder", GenerateSidewinder);
            MazeGenerationDelegateList.Add("GenerateEllers", GenerateEllers);
            MazeGenerationDelegateList.Add("GenerateRecursiveDivision", GenerateRecursiveDivision);
            MazeGenerationDelegateList.Add("GenerateSpiralBacktracker", GenerateSpiralBacktracker);


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

        public void GenerateWilsons(int width, int height)
        {
            Cells = InitializeMaze(width, height, 0);
            Width = width;
            Height = height;

            var visited = new System.Collections.Generic.HashSet<(int, int)>();
            var remaining = new List<(int, int)>();
            for (int row = 0; row < height; row++)
                for (int col = 0; col < width; col++)
                    remaining.Add((col, row));

            (int, int) seed = RandomStart();
            visited.Add(seed);
            remaining.Remove(seed);

            Start = (seed.Item1, seed.Item2);
            Player.X = seed.Item1;
            Player.Y = seed.Item2;
            Cells[seed.Item2][seed.Item1].Value = 2;

            while (remaining.Count > 0)
            {
                (int, int) walkStart = remaining[rnd.Next(remaining.Count)];
                var path = new List<(int, int)> { walkStart };
                var pathIndex = new Dictionary<(int, int), int> { { walkStart, 0 } };
                (int, int) current = walkStart;

                while (!visited.Contains(current))
                {
                    var allNeighbors = new List<(int, int)> { North(current), South(current), East(current), West(current) }
                        .Where(n => InBounds(n)).ToList();
                    (int, int) next = allNeighbors[rnd.Next(allNeighbors.Count)];

                    if (pathIndex.ContainsKey(next))
                    {
                        int loopStart = pathIndex[next] + 1;
                        for (int i = loopStart; i < path.Count; i++)
                            pathIndex.Remove(path[i]);
                        path.RemoveRange(loopStart, path.Count - loopStart);
                    }
                    else
                    {
                        pathIndex[next] = path.Count;
                        path.Add(next);
                    }
                    current = next;
                }

                for (int i = 0; i < path.Count - 1; i++)
                {
                    LinkCells(path[i], path[i + 1]);
                    visited.Add(path[i]);
                    remaining.Remove(path[i]);
                }
            }

            OptimizeMaze();
        }

        public void GenerateAldousBroder(int width, int height)
        {
            Cells = InitializeMaze(width, height, 0);
            Width = width;
            Height = height;

            var visited = new System.Collections.Generic.HashSet<(int, int)>();
            (int, int) current = RandomStart();
            visited.Add(current);
            int totalCells = width * height;

            Start = (current.Item1, current.Item2);
            Player.X = current.Item1;
            Player.Y = current.Item2;
            Cells[current.Item2][current.Item1].Value = 2;

            while (visited.Count < totalCells)
            {
                var neighbors = new List<(int, int)> { North(current), South(current), East(current), West(current) }
                    .Where(n => InBounds(n)).ToList();
                (int, int) next = neighbors[rnd.Next(neighbors.Count)];

                if (!visited.Contains(next))
                {
                    LinkCells(current, next);
                    visited.Add(next);
                }
                current = next;
            }

            OptimizeMaze();
        }

        public void GenerateBinaryTree(int width, int height)
        {
            Cells = InitializeMaze(width, height, 0);
            Width = width;
            Height = height;

            (int, int) startCell = RandomStart();
            Start = (startCell.Item1, startCell.Item2);
            Player.X = startCell.Item1;
            Player.Y = startCell.Item2;
            Cells[startCell.Item2][startCell.Item1].Value = 2;

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    var options = new List<(int, int)>();
                    if (row > 0) options.Add(North((col, row)));
                    if (col < width - 1) options.Add(East((col, row)));

                    if (options.Count > 0)
                        LinkCells((col, row), options[rnd.Next(options.Count)]);
                }
            }

            OptimizeMaze();
        }

        public void GenerateSidewinder(int width, int height)
        {
            Cells = InitializeMaze(width, height, 0);
            Width = width;
            Height = height;

            (int, int) startCell = RandomStart();
            Start = (startCell.Item1, startCell.Item2);
            Player.X = startCell.Item1;
            Player.Y = startCell.Item2;
            Cells[startCell.Item2][startCell.Item1].Value = 2;

            // Top row: link all east
            for (int col = 0; col < width - 1; col++)
                LinkCells((col, 0), East((col, 0)));

            for (int row = 1; row < height; row++)
            {
                var run = new List<(int, int)>();
                for (int col = 0; col < width; col++)
                {
                    run.Add((col, row));
                    bool atEastBoundary = col == width - 1;
                    bool closeRun = atEastBoundary || rnd.Next(2) == 0;

                    if (closeRun)
                    {
                        var northCell = run[rnd.Next(run.Count)];
                        LinkCells(northCell, North(northCell));
                        run.Clear();
                    }
                    else
                    {
                        LinkCells((col, row), East((col, row)));
                    }
                }
            }

            OptimizeMaze();
        }

        public void GenerateEllers(int width, int height)
        {
            Cells = InitializeMaze(width, height, 0);
            Width = width;
            Height = height;

            (int, int) startCell = RandomStart();
            Start = (startCell.Item1, startCell.Item2);
            Player.X = startCell.Item1;
            Player.Y = startCell.Item2;
            Cells[startCell.Item2][startCell.Item1].Value = 2;

            int nextSetId = 0;
            int[] rowSets = new int[width];
            for (int col = 0; col < width; col++)
                rowSets[col] = nextSetId++;

            for (int row = 0; row < height; row++)
            {
                bool isLastRow = row == height - 1;

                // Step 1: merge adjacent cells from different sets
                for (int col = 0; col < width - 1; col++)
                {
                    if (rowSets[col] != rowSets[col + 1] && (isLastRow || rnd.Next(2) == 0))
                    {
                        int mergeFrom = rowSets[col + 1];
                        int mergeTo = rowSets[col];
                        for (int c = 0; c < width; c++)
                            if (rowSets[c] == mergeFrom) rowSets[c] = mergeTo;
                        LinkCells((col, row), East((col, row)));
                    }
                }

                if (!isLastRow)
                {
                    // Step 2: for each set ensure at least 1 south connection
                    var sets = new Dictionary<int, List<int>>();
                    for (int col = 0; col < width; col++)
                    {
                        if (!sets.ContainsKey(rowSets[col])) sets[rowSets[col]] = new List<int>();
                        sets[rowSets[col]].Add(col);
                    }

                    int[] nextRowSets = new int[width];
                    for (int i = 0; i < width; i++) nextRowSets[i] = -1;

                    foreach (var kv in sets)
                    {
                        var shuffledCols = kv.Value.OrderBy(_ => rnd.Next()).ToList();
                        bool hasConnected = false;
                        for (int i = 0; i < shuffledCols.Count; i++)
                        {
                            int col = shuffledCols[i];
                            bool isLastInSet = i == shuffledCols.Count - 1;
                            bool connectSouth = (!hasConnected && isLastInSet) || rnd.Next(2) == 0;
                            if (connectSouth)
                            {
                                LinkCells((col, row), South((col, row)));
                                nextRowSets[col] = kv.Key;
                                hasConnected = true;
                            }
                        }
                    }

                    for (int col = 0; col < width; col++)
                        if (nextRowSets[col] == -1) nextRowSets[col] = nextSetId++;

                    rowSets = nextRowSets;
                }
            }

            OptimizeMaze();
        }

        private void RecursiveDivide(int colStart, int colEnd, int rowStart, int rowEnd)
        {
            int w = colEnd - colStart + 1;
            int h = rowEnd - rowStart + 1;
            if (w < 2 || h < 2) return;

            bool horizontal;
            if (h > w) horizontal = true;
            else if (w > h) horizontal = false;
            else horizontal = rnd.Next(2) == 0;

            if (horizontal)
            {
                int wallRow = rnd.Next(rowStart, rowEnd);
                int gapCol = rnd.Next(colStart, colEnd + 1);
                for (int col = colStart; col <= colEnd; col++)
                    if (col != gapCol)
                        Cells[wallRow + 1][col].North = true;
                RecursiveDivide(colStart, colEnd, rowStart, wallRow);
                RecursiveDivide(colStart, colEnd, wallRow + 1, rowEnd);
            }
            else
            {
                int wallCol = rnd.Next(colStart, colEnd);
                int gapRow = rnd.Next(rowStart, rowEnd + 1);
                for (int row = rowStart; row <= rowEnd; row++)
                    if (row != gapRow)
                        Cells[row][wallCol].East = true;
                RecursiveDivide(colStart, wallCol, rowStart, rowEnd);
                RecursiveDivide(wallCol + 1, colEnd, rowStart, rowEnd);
            }
        }

        public void GenerateRecursiveDivision(int width, int height)
        {
            Cells = InitializeMaze(width, height, 0);
            Width = width;
            Height = height;

            // Open all interior walls — start with a completely open grid
            for (int row = 0; row < height; row++)
                for (int col = 0; col < width; col++)
                {
                    if (col < width - 1) Cells[row][col].East = false;
                    if (row > 0) Cells[row][col].North = false;
                }

            (int, int) startCell = RandomStart();
            Start = (startCell.Item1, startCell.Item2);
            Player.X = startCell.Item1;
            Player.Y = startCell.Item2;
            Cells[startCell.Item2][startCell.Item1].Value = 2;

            RecursiveDivide(0, width - 1, 0, height - 1);
            OptimizeMaze();
        }

        public void GenerateSpiralBacktracker(int width, int height)
        {
            Cells = InitializeMaze(width, height, 0);
            Width = width;
            Height = height;

            var visited = new Dictionary<(int, int), bool>();
            for (int row = 0; row < height; row++)
                for (int col = 0; col < width; col++)
                    visited[(col, row)] = false;

            (int, int) startCell = RandomStart();
            Start = (startCell.Item1, startCell.Item2);
            Player.X = startCell.Item1;
            Player.Y = startCell.Item2;
            Cells[startCell.Item2][startCell.Item1].Value = 2;

            visited[startCell] = true;
            var stack = new System.Collections.Generic.Stack<(int, int)>();
            stack.Push(startCell);
            (int, int) lastDir = (0, 0);
            const int SPIRAL_WEIGHT = 70;

            while (stack.Count > 0)
            {
                (int, int) current = stack.Peek();
                var unvisited = AdjacentNotVisited(visited, current);

                if (unvisited.Count == 0)
                {
                    stack.Pop();
                    lastDir = (0, 0);
                    continue;
                }

                (int, int) next;
                (int, int) preferred = (current.Item1 + lastDir.Item1, current.Item2 + lastDir.Item2);
                if (lastDir != (0, 0) && unvisited.Contains(preferred) && rnd.Next(100) < SPIRAL_WEIGHT)
                    next = preferred;
                else
                    next = unvisited[rnd.Next(unvisited.Count)];

                lastDir = (next.Item1 - current.Item1, next.Item2 - current.Item2);
                LinkCells(current, next);
                visited[next] = true;
                stack.Push(next);
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
