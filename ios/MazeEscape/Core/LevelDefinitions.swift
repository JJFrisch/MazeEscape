import Foundation

// MARK: - Campaign Levels

struct LevelDefinitions {

    static func getWorld1() -> CampaignWorld {
        let levels: [CampaignLevel] = [
            level(1, "1", 4, 4, .backtracking, 20, 20, connect: "2"),
            level(2, "2", 4, 4, .huntAndKill, 22, 20, connect: "3"),
            level(3, "3", 4, 5, .backtracking, 25, 25, connect: "4"),
            level(4, "4", 5, 4, .huntAndKill, 28, 25, connect: "5"),
            level(5, "5", 5, 5, .prims, 30, 30, connect: "6"),
            level(6, "6", 5, 5, .kruskals, 32, 30, connect: "7"),
            level(7, "7", 5, 6, .growingTree_50_50, 35, 35, connect: "8"),
            level(8, "8", 6, 5, .backtracking, 38, 35, connect: "9"),
            level(9, "9", 6, 6, .huntAndKill, 42, 40, connect: "10"),
            level(10, "10", 6, 6, .prims, 45, 40, connect: "11"),
            level(11, "11", 6, 7, .kruskals, 48, 45, connect: "12"),
            level(12, "12", 7, 6, .growingTree_75_25, 52, 45, connect: "13"),
            level(13, "13", 7, 7, .backtracking, 56, 50, connect: "14"),
            level(14, "14", 7, 7, .huntAndKill, 60, 50, connect: "15"),
            level(15, "15", 7, 8, .prims, 65, 55, connect: "16"),
            level(16, "16", 8, 7, .kruskals, 68, 55, connect: "17"),
            level(17, "17", 8, 8, .growingTree_25_75, 72, 60, connect: "18"),
            level(18, "18", 8, 8, .backtracking, 76, 60, connect: "19"),
            level(19, "19", 8, 9, .huntAndKill, 80, 65, connect: "20"),
            level(20, "20", 9, 8, .prims, 84, 65, connect: "21"),
        ]

        return CampaignWorld(
            worldId: 1,
            worldName: "Classic Maze",
            levels: levels,
            gateStarRequired: [20, 45, 30, 60, 80, 100, 120, 150, 200, 230, 240, 250]
        )
    }

    static func getWorld2() -> CampaignWorld {
        let levels: [CampaignLevel] = [
            level(1, "1", 6, 6, .backtracking, 45, 35, connect: "2"),
            level(2, "2", 6, 7, .huntAndKill, 50, 40, connect: "3"),
            level(3, "3", 7, 7, .prims, 55, 45, connect: "4"),
            level(4, "4", 7, 8, .kruskals, 60, 50, connect: "5"),
            level(5, "5", 8, 8, .growingTree_50_50, 70, 55, connect: "6"),
            level(6, "6", 8, 9, .backtracking, 75, 60, connect: "7"),
            level(7, "7", 9, 9, .huntAndKill, 85, 65, connect: "8"),
            level(8, "8", 9, 10, .prims, 90, 70, connect: "9"),
            level(9, "9", 10, 10, .kruskals, 100, 75, connect: "10"),
            level(10, "10", 10, 10, .growingTree_75_25, 110, 80, connect: "11"),
        ]

        return CampaignWorld(
            worldId: 2,
            worldName: "Shadow Labyrinth",
            levels: levels,
            gateStarRequired: [20, 30, 50, 60, 80, 100, 120, 150, 230, 240, 250, 150, 230, 240, 250, 240, 250]
        )
    }

    static func getAllWorlds() -> [CampaignWorld] {
        [getWorld1(), getWorld2()]
    }

    // MARK: - Helper

    private static func level(_ id: Int, _ number: String, _ w: Int, _ h: Int, _ algo: MazeAlgorithm, _ moves: Int, _ time: Int, connect: String? = nil) -> CampaignLevel {
        CampaignLevel(
            levelId: id,
            levelNumber: number,
            width: w,
            height: h,
            levelType: algo.rawValue,
            twoStarMoves: moves,
            threeStarTime: time,
            numberOfStars: 0,
            minimumStarsToUnlock: 0,
            connectTo1: connect,
            connectTo2: nil,
            completed: false,
            star1: false,
            star2: false,
            star3: false,
            bestMoves: 0,
            bestTime: 0
        )
    }
}

// MARK: - Skin Catalog

struct SkinCatalog {
    static let all: [SkinModel] = [
        SkinModel(id: 0, name: "Maze Solver", coinPrice: 0, isSpecialSkin: false),
        SkinModel(id: 1, name: "Cool Lion", coinPrice: 500, isSpecialSkin: false),
        SkinModel(id: 2, name: "Sunset Swirl", coinPrice: 1000, isSpecialSkin: false),
        SkinModel(id: 3, name: "Pink Sunset", coinPrice: 2000, isSpecialSkin: false),
        SkinModel(id: 4, name: "Diskes", coinPrice: 5000, isSpecialSkin: false),
        SkinModel(id: 5, name: "Fireball", coinPrice: 22000, isSpecialSkin: false),
        SkinModel(id: 6, name: "Galaxy Ball", coinPrice: 50000, isSpecialSkin: false),
        SkinModel(id: 7, name: "Elemental Mixer", coinPrice: 6500, isSpecialSkin: false),
        SkinModel(id: 8, name: "Ivy Eye", coinPrice: 3400, isSpecialSkin: false),
        SkinModel(id: 9, name: "Ruffles", coinPrice: 7120, isSpecialSkin: false),
        SkinModel(id: 10, name: "Rocco", coinPrice: 3000, isSpecialSkin: false),
        SkinModel(id: 11, name: "Pugsley", coinPrice: 4375, isSpecialSkin: false),
        SkinModel(id: 12, name: "Kowalski", coinPrice: 9000, isSpecialSkin: false),
        SkinModel(id: 13, name: "Space Maze", coinPrice: 0, isSpecialSkin: true),
        SkinModel(id: 14, name: "Brain", coinPrice: 19000, isSpecialSkin: false),
        SkinModel(id: 15, name: "Chucky", coinPrice: 0, isSpecialSkin: true),
        SkinModel(id: 16, name: "Fire Elemental", coinPrice: 8500, isSpecialSkin: false),
        SkinModel(id: 17, name: "Da Butler", coinPrice: 0, isSpecialSkin: true),
    ]
}
