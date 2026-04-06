import SwiftUI

struct LandingView: View {
    @EnvironmentObject var store: GameStore

    var body: some View {
        ScrollView {
            VStack(spacing: 32) {
                // Hero
                VStack(spacing: 12) {
                    Text("🔮")
                        .font(.system(size: 60))
                    Text("MazeEscape")
                        .font(.largeTitle.bold())
                    Text("Navigate. Solve. Escape.")
                        .foregroundStyle(.secondary)
                }
                .padding(.top, 40)

                // Coin balance
                HStack {
                    Image(systemName: "dollarsign.circle.fill")
                        .foregroundStyle(.yellow)
                    Text("\(store.player.coinCount)")
                        .font(.title2.bold())
                }
                .padding()
                .background(.ultraThinMaterial, in: RoundedRectangle(cornerRadius: 16))

                // Quick actions
                VStack(spacing: 16) {
                    NavigationLink(destination: WorldsView()) {
                        ActionCard(title: "Campaign", icon: "map.fill", color: .indigo)
                    }
                    NavigationLink(destination: DailyMazeView()) {
                        ActionCard(title: "Daily Maze", icon: "calendar", color: .green)
                    }
                }
                .padding(.horizontal)

                // Stats
                HStack(spacing: 24) {
                    StatBadge(label: "Worlds", value: "\(LevelDefinitions.getAllWorlds().count)")
                    StatBadge(label: "Skins", value: "\(store.player.unlockedSkinIds.count)")
                    StatBadge(label: "Hints", value: "\(store.player.hintsOwned)")
                }
            }
            .padding(.bottom, 40)
        }
        .navigationTitle("Home")
    }
}

// MARK: - Components

private struct ActionCard: View {
    let title: String
    let icon: String
    let color: Color

    var body: some View {
        HStack {
            Image(systemName: icon)
                .font(.title2)
                .foregroundStyle(color)
                .frame(width: 40)
            Text(title)
                .font(.headline)
                .foregroundStyle(.primary)
            Spacer()
            Image(systemName: "chevron.right")
                .foregroundStyle(.secondary)
        }
        .padding()
        .background(.ultraThinMaterial, in: RoundedRectangle(cornerRadius: 16))
    }
}

private struct StatBadge: View {
    let label: String
    let value: String

    var body: some View {
        VStack(spacing: 4) {
            Text(value)
                .font(.title2.bold())
            Text(label)
                .font(.caption)
                .foregroundStyle(.secondary)
        }
    }
}
