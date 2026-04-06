import Foundation

/// Manages a single maze playthrough.
class GameSession: ObservableObject {
    let maze: MazeData
    @Published var playerPos: Position
    @Published var moves: Int = 0
    @Published var elapsed: Double = 0
    @Published var isComplete: Bool = false
    @Published var hintPath: [Position]? = nil

    let twoStarMoves: Int
    let threeStarTime: Int

    private var timer: Timer?

    init(width: Int, height: Int, algorithm: MazeAlgorithm, seed: Int, twoStarMoves: Int, threeStarTime: Int) {
        self.maze = MazeGenerator.generate(width: width, height: height, algorithm: algorithm, seed: seed)
        self.playerPos = maze.start
        self.twoStarMoves = twoStarMoves
        self.threeStarTime = threeStarTime
    }

    func startTimer() {
        timer?.invalidate()
        timer = Timer.scheduledTimer(withTimeInterval: 0.01, repeats: true) { [weak self] _ in
            guard let self, !self.isComplete else { return }
            self.elapsed += 0.01
        }
    }

    func stopTimer() {
        timer?.invalidate()
        timer = nil
    }

    func tryMove(_ direction: Direction) -> Bool {
        guard !isComplete else { return false }
        guard MazeGenerator.canMove(cells: maze.cells, from: playerPos, direction: direction, width: maze.width, height: maze.height) else {
            return false
        }
        playerPos = Position(x: playerPos.x + direction.dx, y: playerPos.y + direction.dy)
        moves += 1
        hintPath = nil

        if playerPos == maze.end {
            isComplete = true
            stopTimer()
        }
        return true
    }

    func getHint() {
        let path = MazeGenerator.findPath(cells: maze.cells, from: playerPos, to: maze.end, width: maze.width, height: maze.height)
        hintPath = path
    }

    func calculateStars() -> (star1: Bool, star2: Bool, star3: Bool, total: Int) {
        let s1 = isComplete
        let s2 = s1 && moves <= twoStarMoves
        let s3 = s1 && elapsed <= Double(threeStarTime)
        return (s1, s2, s3, (s1 ? 1 : 0) + (s2 ? 1 : 0) + (s3 ? 1 : 0))
    }

    deinit {
        stopTimer()
    }
}
