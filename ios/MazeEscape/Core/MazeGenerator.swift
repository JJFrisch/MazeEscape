import Foundation

// MARK: - Maze Generator

class MazeGenerator {

    // MARK: - Public API

    static func generate(width: Int, height: Int, algorithm: MazeAlgorithm, seed: Int) -> MazeData {
        let rng = SeededRandom(seed: seed)
        var cells = initializeGrid(width: width, height: height)

        switch algorithm {
        case .backtracking:
            generateBacktracking(&cells, width: width, height: height, rng: rng)
        case .huntAndKill:
            generateHuntAndKill(&cells, width: width, height: height, rng: rng)
        case .prims:
            generatePrims(&cells, width: width, height: height, rng: rng)
        case .kruskals:
            generateKruskals(&cells, width: width, height: height, rng: rng)
        case .growingTree_50_50:
            generateGrowingTree(&cells, width: width, height: height, rng: rng, newestPct: 50, randomPct: 50)
        case .growingTree_75_25:
            generateGrowingTree(&cells, width: width, height: height, rng: rng, newestPct: 75, randomPct: 25)
        case .growingTree_25_75:
            generateGrowingTree(&cells, width: width, height: height, rng: rng, newestPct: 25, randomPct: 75)
        case .growingTree_50_0:
            generateGrowingTree(&cells, width: width, height: height, rng: rng, newestPct: 50, randomPct: 0)
        }

        // Optimize: BFS for furthest cell as exit
        let start = Position(x: 0, y: 0)
        let end = findFurthestCell(cells: cells, from: start, width: width, height: height)
        let path = findPath(cells: cells, from: start, to: end, width: width, height: height)

        cells[start.y][start.x].value = 2
        cells[end.y][end.x].value = 3

        return MazeData(
            cells: cells,
            width: width,
            height: height,
            start: start,
            end: end,
            path: path,
            pathLength: path.count
        )
    }

    // MARK: - Grid Initialization

    private static func initializeGrid(width: Int, height: Int) -> [[MazeCell]] {
        (0..<height).map { y in
            (0..<width).map { x in
                MazeCell(x: x, y: y, value: 0, east: true, north: true)
            }
        }
    }

    // MARK: - Wall Links

    private static func linkCells(_ cells: inout [[MazeCell]], _ a: Position, _ b: Position) {
        if a.x == b.x {
            if a.y < b.y {
                cells[b.y][b.x].north = false
            } else {
                cells[a.y][a.x].north = false
            }
        } else {
            if a.x < b.x {
                cells[a.y][a.x].east = false
            } else {
                cells[b.y][b.x].east = false
            }
        }
    }

    // MARK: - Neighbour Helpers

    private static func getUnvisitedNeighbours(_ cells: [[MazeCell]], _ pos: Position, _ visited: Set<Position>, width: Int, height: Int) -> [Position] {
        var result: [Position] = []
        for dir in Direction.allCases {
            let nx = pos.x + dir.dx
            let ny = pos.y + dir.dy
            let np = Position(x: nx, y: ny)
            if nx >= 0 && nx < width && ny >= 0 && ny < height && !visited.contains(np) {
                result.append(np)
            }
        }
        return result
    }

    private static func getAccessibleNeighbours(_ cells: [[MazeCell]], _ pos: Position, width: Int, height: Int) -> [Position] {
        var result: [Position] = []
        // Up: check north wall of current cell
        if pos.y > 0 && !cells[pos.y][pos.x].north {
            result.append(Position(x: pos.x, y: pos.y - 1))
        }
        // Down: check north wall of cell below
        if pos.y < height - 1 && !cells[pos.y + 1][pos.x].north {
            result.append(Position(x: pos.x, y: pos.y + 1))
        }
        // Left: check east wall of cell to the left
        if pos.x > 0 && !cells[pos.y][pos.x - 1].east {
            result.append(Position(x: pos.x - 1, y: pos.y))
        }
        // Right: check east wall of current cell
        if pos.x < width - 1 && !cells[pos.y][pos.x].east {
            result.append(Position(x: pos.x + 1, y: pos.y))
        }
        return result
    }

    // MARK: - Algorithms

    private static func generateBacktracking(_ cells: inout [[MazeCell]], width: Int, height: Int, rng: SeededRandom) {
        var visited = Set<Position>()
        var stack: [Position] = []
        let start = Position(x: rng.nextInt(0, width), y: rng.nextInt(0, height))
        visited.insert(start)
        stack.append(start)

        while !stack.isEmpty {
            let current = stack.last!
            var neighbours = getUnvisitedNeighbours(cells, current, visited, width: width, height: height)

            if neighbours.isEmpty {
                stack.removeLast()
            } else {
                rng.shuffle(&neighbours)
                let next = neighbours[0]
                linkCells(&cells, current, next)
                visited.insert(next)
                stack.append(next)
            }
        }
    }

    private static func generateHuntAndKill(_ cells: inout [[MazeCell]], width: Int, height: Int, rng: SeededRandom) {
        var visited = Set<Position>()
        var current = Position(x: rng.nextInt(0, width), y: rng.nextInt(0, height))
        visited.insert(current)

        while true {
            var neighbours = getUnvisitedNeighbours(cells, current, visited, width: width, height: height)

            if !neighbours.isEmpty {
                rng.shuffle(&neighbours)
                let next = neighbours[0]
                linkCells(&cells, current, next)
                visited.insert(next)
                current = next
            } else {
                // Hunt phase
                var found = false
                for y in 0..<height {
                    for x in 0..<width {
                        let pos = Position(x: x, y: y)
                        if visited.contains(pos) { continue }
                        let visitedNeighbours = getUnvisitedNeighbours(cells, pos, Set<Position>(), width: width, height: height)
                            .filter { visited.contains($0) }
                        if !visitedNeighbours.isEmpty {
                            var vn = visitedNeighbours
                            rng.shuffle(&vn)
                            linkCells(&cells, pos, vn[0])
                            visited.insert(pos)
                            current = pos
                            found = true
                            break
                        }
                    }
                    if found { break }
                }
                if !found { break }
            }
        }
    }

    private static func generatePrims(_ cells: inout [[MazeCell]], width: Int, height: Int, rng: SeededRandom) {
        var visited = Set<Position>()
        var frontier: [Position] = []
        let start = Position(x: rng.nextInt(0, width), y: rng.nextInt(0, height))
        visited.insert(start)

        for n in getUnvisitedNeighbours(cells, start, visited, width: width, height: height) {
            frontier.append(n)
        }

        while !frontier.isEmpty {
            let idx = rng.nextInt(0, frontier.count)
            let current = frontier[idx]
            frontier.remove(at: idx)

            if visited.contains(current) { continue }

            var visitedNeighbours = getUnvisitedNeighbours(cells, current, Set<Position>(), width: width, height: height)
                .filter { visited.contains($0) }
            if visitedNeighbours.isEmpty { continue }

            rng.shuffle(&visitedNeighbours)
            linkCells(&cells, current, visitedNeighbours[0])
            visited.insert(current)

            for n in getUnvisitedNeighbours(cells, current, visited, width: width, height: height) {
                if !frontier.contains(n) {
                    frontier.append(n)
                }
            }
        }
    }

    private static func generateKruskals(_ cells: inout [[MazeCell]], width: Int, height: Int, rng: SeededRandom) {
        var parent: [Position: Position] = [:]

        func find(_ p: Position) -> Position {
            var current = p
            while let par = parent[current], par != current {
                current = par
            }
            parent[p] = current
            return current
        }

        func union(_ a: Position, _ b: Position) {
            let ra = find(a), rb = find(b)
            if ra != rb { parent[ra] = rb }
        }

        for y in 0..<height {
            for x in 0..<width {
                let p = Position(x: x, y: y)
                parent[p] = p
            }
        }

        var edges: [(Position, Position)] = []
        for y in 0..<height {
            for x in 0..<width {
                let p = Position(x: x, y: y)
                if x < width - 1 { edges.append((p, Position(x: x + 1, y: y))) }
                if y < height - 1 { edges.append((p, Position(x: x, y: y + 1))) }
            }
        }
        rng.shuffle(&edges)

        for (a, b) in edges {
            if find(a) != find(b) {
                union(a, b)
                linkCells(&cells, a, b)
            }
        }
    }

    private static func generateGrowingTree(_ cells: inout [[MazeCell]], width: Int, height: Int, rng: SeededRandom, newestPct: Int, randomPct: Int) {
        var visited = Set<Position>()
        var active: [Position] = []
        let start = Position(x: rng.nextInt(0, width), y: rng.nextInt(0, height))
        visited.insert(start)
        active.append(start)

        while !active.isEmpty {
            let idx: Int
            let roll = rng.nextInt(0, 100)
            if roll < newestPct {
                idx = active.count - 1
            } else if roll < newestPct + randomPct {
                idx = rng.nextInt(0, active.count)
            } else {
                idx = 0
            }

            let current = active[idx]
            var neighbours = getUnvisitedNeighbours(cells, current, visited, width: width, height: height)

            if neighbours.isEmpty {
                active.remove(at: idx)
            } else {
                rng.shuffle(&neighbours)
                let next = neighbours[0]
                linkCells(&cells, current, next)
                visited.insert(next)
                active.append(next)
            }
        }
    }

    // MARK: - BFS / Pathfinding

    private static func findFurthestCell(cells: [[MazeCell]], from start: Position, width: Int, height: Int) -> Position {
        var visited = Set<Position>()
        var queue: [Position] = [start]
        visited.insert(start)
        var furthest = start

        while !queue.isEmpty {
            let current = queue.removeFirst()
            furthest = current
            for n in getAccessibleNeighbours(cells, current, width: width, height: height) {
                if !visited.contains(n) {
                    visited.insert(n)
                    queue.append(n)
                }
            }
        }
        return furthest
    }

    static func findPath(cells: [[MazeCell]], from start: Position, to end: Position, width: Int, height: Int) -> [Position] {
        var visited = Set<Position>()
        var parent: [Position: Position] = [:]
        var stack: [Position] = [start]
        visited.insert(start)

        while !stack.isEmpty {
            let current = stack.removeLast()
            if current == end {
                // Reconstruct path
                var path: [Position] = []
                var cur: Position? = end
                while let c = cur {
                    path.append(c)
                    cur = parent[c]
                }
                return path.reversed()
            }
            for n in getAccessibleNeighbours(cells, current, width: width, height: height) {
                if !visited.contains(n) {
                    visited.insert(n)
                    parent[n] = current
                    stack.append(n)
                }
            }
        }
        return []
    }

    // MARK: - Movement

    static func canMove(cells: [[MazeCell]], from pos: Position, direction: Direction, width: Int, height: Int) -> Bool {
        let nx = pos.x + direction.dx
        let ny = pos.y + direction.dy
        guard nx >= 0 && nx < width && ny >= 0 && ny < height else { return false }

        switch direction {
        case .up:
            return !cells[pos.y][pos.x].north
        case .down:
            return !cells[ny][nx].north
        case .left:
            return !cells[pos.y][nx].east
        case .right:
            return !cells[pos.y][pos.x].east
        }
    }
}
