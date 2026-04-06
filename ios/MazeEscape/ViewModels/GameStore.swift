import Foundation
import Combine

class GameStore: ObservableObject {
    @Published var player: PlayerData
    @Published var levelProgress: [String: CampaignLevel] = [:]
    @Published var dailyResults: [String: DailyMazeResult] = [:]

    private let playerKey = "mazeescape_player"
    private let levelsKey = "mazeescape_levels"
    private let dailyKey = "mazeescape_daily"

    init() {
        self.player = Self.load(key: "mazeescape_player") ?? PlayerData.defaultPlayer()
        self.levelProgress = Self.load(key: "mazeescape_levels") ?? [:]
        self.dailyResults = Self.load(key: "mazeescape_daily") ?? [:]
    }

    // MARK: - Persistence

    private func save() {
        Self.store(player, key: playerKey)
        Self.store(levelProgress, key: levelsKey)
        Self.store(dailyResults, key: dailyKey)
    }

    private static func load<T: Decodable>(key: String) -> T? {
        guard let data = UserDefaults.standard.data(forKey: key) else { return nil }
        return try? JSONDecoder().decode(T.self, from: data)
    }

    private static func store<T: Encodable>(_ value: T, key: String) {
        guard let data = try? JSONEncoder().encode(value) else { return }
        UserDefaults.standard.set(data, forKey: key)
    }

    // MARK: - Coins

    func addCoins(_ amount: Int) {
        player.coinCount += amount
        save()
    }

    func spendCoins(_ amount: Int) -> Bool {
        guard player.coinCount >= amount else { return false }
        player.coinCount -= amount
        save()
        return true
    }

    // MARK: - Skins

    func equipSkin(_ skinId: Int) {
        guard player.unlockedSkinIds.contains(skinId) else { return }
        player.currentSkinId = skinId
        save()
    }

    func buySkin(_ skinId: Int) -> Bool {
        guard let skin = SkinCatalog.all.first(where: { $0.id == skinId }),
              !skin.isSpecialSkin,
              !player.unlockedSkinIds.contains(skinId),
              spendCoins(skin.coinPrice) else { return false }
        player.unlockedSkinIds.append(skinId)
        save()
        return true
    }

    // MARK: - Powerups

    func addPowerup(_ type: PowerupType) {
        switch type {
        case .hint: player.hintsOwned += 1
        case .extraTime: player.extraTimesOwned += 1
        case .extraMoves: player.extraMovesOwned += 1
        }
        save()
    }

    func usePowerup(_ type: PowerupType) -> Bool {
        switch type {
        case .hint:
            guard player.hintsOwned > 0 else { return false }
            player.hintsOwned -= 1
        case .extraTime:
            guard player.extraTimesOwned > 0 else { return false }
            player.extraTimesOwned -= 1
        case .extraMoves:
            guard player.extraMovesOwned > 0 else { return false }
            player.extraMovesOwned -= 1
        }
        save()
        return true
    }

    // MARK: - Level Progress

    func getLevelProgress(worldId: Int, levelNumber: String) -> CampaignLevel? {
        levelProgress["\(worldId):\(levelNumber)"]
    }

    func saveLevelProgress(worldId: Int, level: CampaignLevel) {
        let key = "\(worldId):\(level.levelNumber)"
        var updated = level
        if let existing = levelProgress[key] {
            updated.star1 = updated.star1 || existing.star1
            updated.star2 = updated.star2 || existing.star2
            updated.star3 = updated.star3 || existing.star3
            updated.numberOfStars = (updated.star1 ? 1 : 0) + (updated.star2 ? 1 : 0) + (updated.star3 ? 1 : 0)
            updated.completed = updated.completed || existing.completed
            if existing.bestMoves > 0 { updated.bestMoves = min(updated.bestMoves, existing.bestMoves) }
            if existing.bestTime > 0 { updated.bestTime = min(updated.bestTime, existing.bestTime) }
        }
        levelProgress[key] = updated
        save()
    }

    func worldStarCount(_ worldId: Int) -> Int {
        levelProgress
            .filter { $0.key.hasPrefix("\(worldId):") }
            .reduce(0) { $0 + $1.value.numberOfStars }
    }

    // MARK: - Daily

    func saveDailyResult(_ result: DailyMazeResult) {
        dailyResults[result.shortDate] = result
        save()
    }

    func getDailyResult(_ shortDate: String) -> DailyMazeResult? {
        dailyResults[shortDate]
    }

    func isDailyUnlocked() -> Bool {
        getLevelProgress(worldId: 1, levelNumber: "10")?.star1 == true
    }
}

// MARK: - Powerup Type

enum PowerupType {
    case hint, extraTime, extraMoves

    var cost: Int {
        switch self {
        case .hint: return 200
        case .extraTime: return 150
        case .extraMoves: return 50
        }
    }

    var displayName: String {
        switch self {
        case .hint: return "Hint"
        case .extraTime: return "Extra Time"
        case .extraMoves: return "Extra Moves"
        }
    }
}
