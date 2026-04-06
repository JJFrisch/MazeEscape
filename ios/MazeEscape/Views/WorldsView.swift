import SwiftUI

struct WorldsView: View {
    @EnvironmentObject var store: GameStore
    let worlds = LevelDefinitions.getAllWorlds()

    var body: some View {
        ScrollView {
            LazyVStack(spacing: 16) {
                ForEach(worlds) { world in
                    NavigationLink(destination: WorldDetailView(world: world)) {
                        WorldCard(world: world, starCount: store.worldStarCount(world.worldId))
                    }
                    .buttonStyle(.plain)
                }
            }
            .padding()
        }
        .navigationTitle("Worlds")
    }
}

private struct WorldCard: View {
    let world: CampaignWorld
    let starCount: Int

    var body: some View {
        VStack(alignment: .leading, spacing: 8) {
            HStack {
                Text(world.worldName)
                    .font(.headline)
                Spacer()
                HStack(spacing: 4) {
                    Image(systemName: "star.fill")
                        .foregroundStyle(.yellow)
                    Text("\(starCount)")
                        .font(.subheadline.bold())
                }
            }
            Text("\(world.levels.count) levels")
                .font(.caption)
                .foregroundStyle(.secondary)
        }
        .padding()
        .background(.ultraThinMaterial, in: RoundedRectangle(cornerRadius: 16))
    }
}
