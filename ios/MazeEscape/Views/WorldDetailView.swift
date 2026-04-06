import SwiftUI

struct WorldDetailView: View {
    @EnvironmentObject var store: GameStore
    let world: CampaignWorld

    var mainLevels: [CampaignLevel] {
        world.levels.filter { !$0.levelNumber.contains("b") }
    }

    var bonusLevels: [CampaignLevel] {
        world.levels.filter { $0.levelNumber.contains("b") }
    }

    var body: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 24) {
                // Star count
                HStack {
                    Image(systemName: "star.fill")
                        .foregroundStyle(.yellow)
                    Text("\(store.worldStarCount(world.worldId)) stars earned")
                        .font(.subheadline)
                        .foregroundStyle(.secondary)
                }

                // Main levels
                Text("Levels")
                    .font(.headline)

                LazyVGrid(columns: [GridItem(.adaptive(minimum: 60))], spacing: 12) {
                    ForEach(mainLevels) { level in
                        let progress = store.getLevelProgress(worldId: world.worldId, levelNumber: level.levelNumber)
                        NavigationLink(destination: GameplayView(worldId: world.worldId, level: level)) {
                            LevelCell(level: level, progress: progress)
                        }
                        .buttonStyle(.plain)
                    }
                }

                if !bonusLevels.isEmpty {
                    Text("Bonus Levels")
                        .font(.headline)

                    LazyVGrid(columns: [GridItem(.adaptive(minimum: 60))], spacing: 12) {
                        ForEach(bonusLevels) { level in
                            let progress = store.getLevelProgress(worldId: world.worldId, levelNumber: level.levelNumber)
                            NavigationLink(destination: GameplayView(worldId: world.worldId, level: level)) {
                                LevelCell(level: level, progress: progress)
                            }
                            .buttonStyle(.plain)
                        }
                    }
                }
            }
            .padding()
        }
        .navigationTitle(world.worldName)
    }
}

private struct LevelCell: View {
    let level: CampaignLevel
    let progress: CampaignLevel?

    var stars: Int { progress?.numberOfStars ?? 0 }

    var body: some View {
        VStack(spacing: 4) {
            Text(level.levelNumber)
                .font(.headline)
            HStack(spacing: 2) {
                ForEach(0..<3, id: \.self) { i in
                    Image(systemName: i < stars ? "star.fill" : "star")
                        .font(.system(size: 8))
                        .foregroundStyle(i < stars ? .yellow : .secondary)
                }
            }
        }
        .frame(width: 60, height: 60)
        .background(
            progress?.completed == true
                ? Color.green.opacity(0.15)
                : Color(uiColor: .secondarySystemBackground)
        )
        .clipShape(RoundedRectangle(cornerRadius: 12))
        .overlay(
            RoundedRectangle(cornerRadius: 12)
                .stroke(progress?.completed == true ? .green.opacity(0.5) : .clear, lineWidth: 1)
        )
    }
}
