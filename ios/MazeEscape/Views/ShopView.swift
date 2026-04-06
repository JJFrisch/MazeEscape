import SwiftUI

struct ShopView: View {
    @EnvironmentObject var store: GameStore
    @State private var toast: String?

    var body: some View {
        ScrollView {
            VStack(spacing: 20) {
                // Balance
                HStack {
                    Text("Balance")
                        .foregroundStyle(.secondary)
                        .textCase(.uppercase)
                        .font(.caption)
                    Spacer()
                    Text("\(store.player.coinCount) 🪙")
                        .font(.title2.bold())
                        .foregroundStyle(.yellow)
                }
                .padding()
                .background(.ultraThinMaterial, in: RoundedRectangle(cornerRadius: 16))

                if let toast {
                    Text(toast)
                        .font(.caption.bold())
                        .foregroundStyle(.black)
                        .padding(.horizontal, 16)
                        .padding(.vertical, 8)
                        .background(.green, in: Capsule())
                }

                ForEach(PowerupType.allCases, id: \.self) { type in
                    powerupCard(type)
                }
            }
            .padding()
        }
        .navigationTitle("Shop")
    }

    private func powerupCard(_ type: PowerupType) -> some View {
        let owned = ownedCount(type)
        let canAfford = store.player.coinCount >= type.cost

        return HStack {
            VStack(alignment: .leading, spacing: 4) {
                Text(type.displayName)
                    .font(.headline)
                Text("Owned: \(owned)")
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }
            Spacer()
            Button {
                if store.spendCoins(type.cost) {
                    store.addPowerup(type)
                    toast = "Bought \(type.displayName)!"
                    DispatchQueue.main.asyncAfter(deadline: .now() + 2) { toast = nil }
                }
            } label: {
                Text("\(type.cost) 🪙")
                    .font(.subheadline.bold())
            }
            .buttonStyle(.borderedProminent)
            .tint(.indigo)
            .disabled(!canAfford)
        }
        .padding()
        .background(.ultraThinMaterial, in: RoundedRectangle(cornerRadius: 16))
    }

    private func ownedCount(_ type: PowerupType) -> Int {
        switch type {
        case .hint: return store.player.hintsOwned
        case .extraTime: return store.player.extraTimesOwned
        case .extraMoves: return store.player.extraMovesOwned
        }
    }
}

extension PowerupType: CaseIterable {
    static var allCases: [PowerupType] { [.hint, .extraTime, .extraMoves] }
}
