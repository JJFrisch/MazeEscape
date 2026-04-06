import Foundation

// MARK: - Maze Cell

struct MazeCell {
    var x: Int
    var y: Int
    /// 0=empty, 2=start, 3=end
    var value: Int
    /// Wall to the east (right)
    var east: Bool
    /// Wall to the north (top)
    var north: Bool
}

// MARK: - Position & Direction

struct Position: Equatable, Hashable {
    var x: Int
    var y: Int
}

enum Direction: CaseIterable {
    case up, down, left, right

    var dx: Int {
        switch self {
        case .left: return -1
        case .right: return 1
        default: return 0
        }
    }

    var dy: Int {
        switch self {
        case .up: return -1
        case .down: return 1
        default: return 0
        }
    }
}

// MARK: - Maze Data

struct MazeData {
    var cells: [[MazeCell]]
    var width: Int
    var height: Int
    var start: Position
    var end: Position
    var path: [Position]
    var pathLength: Int
}

// MARK: - Move Result

struct MoveResult {
    var moved: Bool
    var position: Position
}

// MARK: - Maze Algorithm

enum MazeAlgorithm: String, CaseIterable {
    case backtracking
    case huntAndKill
    case prims
    case kruskals
    case growingTree_50_50
    case growingTree_75_25
    case growingTree_25_75
    case growingTree_50_0
}

// MARK: - Campaign Level

struct CampaignLevel: Codable, Identifiable {
    var id: Int { levelId }
    var levelId: Int
    var levelNumber: String
    var width: Int
    var height: Int
    var levelType: String // MazeAlgorithm raw value
    var twoStarMoves: Int
    var threeStarTime: Int
    var numberOfStars: Int
    var minimumStarsToUnlock: Int
    var connectTo1: String?
    var connectTo2: String?
    var completed: Bool
    var star1: Bool
    var star2: Bool
    var star3: Bool
    var bestMoves: Int
    var bestTime: Double
}

// MARK: - Campaign World

struct CampaignWorld: Identifiable {
    var id: Int { worldId }
    var worldId: Int
    var worldName: String
    var levels: [CampaignLevel]
    var gateStarRequired: [Int]
}

// MARK: - Skin

struct SkinModel: Codable, Identifiable {
    var id: Int
    var name: String
    var coinPrice: Int
    var isSpecialSkin: Bool
}

// MARK: - Player Data

struct PlayerData: Codable {
    var playerId: String
    var playerName: String
    var coinCount: Int
    var hintsOwned: Int
    var extraTimesOwned: Int
    var extraMovesOwned: Int
    var currentWorldIndex: Int
    var currentSkinId: Int
    var unlockedSkinIds: [Int]
    var wallColor: String

    static func defaultPlayer() -> PlayerData {
        PlayerData(
            playerId: UUID().uuidString,
            playerName: "Player",
            coinCount: 0,
            hintsOwned: 0,
            extraTimesOwned: 0,
            extraMovesOwned: 0,
            currentWorldIndex: 0,
            currentSkinId: 0,
            unlockedSkinIds: [0],
            wallColor: "#6366F1"
        )
    }
}

// MARK: - Daily Maze Result

struct DailyMazeResult: Codable {
    var shortDate: String
    var completed: Bool
    var completionTime: Double
    var completionMoves: Int
}
