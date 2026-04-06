import SwiftUI

struct EquipView: View {
    @EnvironmentObject var store: GameStore
    @State private var confirmSkin: SkinModel?

    let columns = [GridItem(.adaptive(minimum: 90))]

    var body: some View {
        ScrollView {
            LazyVGrid(columns: columns, spacing: 12) {
                ForEach(SkinCatalog.all) { skin in
                    let owned = store.player.unlockedSkinIds.contains(skin.id)
                    let equipped = store.player.currentSkinId == skin.id

                    Button {
                        if owned {
                            store.equipSkin(skin.id)
                        } else if skin.coinPrice > 0 {
                            confirmSkin = skin
                        }
                    } label: {
                        VStack(spacing: 6) {
                            Circle()
                                .fill(equipped ? .indigo : Color(uiColor: .tertiarySystemBackground))
                                .frame(width: 50, height: 50)
                                .overlay {
                                    Text("🟢")
                                }

                            Text(skin.name)
                                .font(.caption2)
                                .lineLimit(1)

                            if equipped {
                                Text("Equipped")
                                    .font(.system(size: 9).bold())
                                    .foregroundStyle(.indigo)
                            } else if owned {
                                Text("Owned")
                                    .font(.system(size: 9))
                                    .foregroundStyle(.green)
                            } else if skin.coinPrice > 0 {
                                Text("\(skin.coinPrice) 🪙")
                                    .font(.system(size: 9).bold())
                                    .foregroundStyle(.yellow)
                            } else {
                                Text("🔒 Special")
                                    .font(.system(size: 9))
                                    .foregroundStyle(.secondary)
                            }
                        }
                        .frame(maxWidth: .infinity)
                        .padding(.vertical, 8)
                        .background(.ultraThinMaterial, in: RoundedRectangle(cornerRadius: 12))
                        .overlay(
                            RoundedRectangle(cornerRadius: 12)
                                .stroke(equipped ? .indigo : .clear, lineWidth: 2)
                        )
                        .opacity(!owned && skin.isSpecialSkin ? 0.5 : 1)
                    }
                    .buttonStyle(.plain)
                }
            }
            .padding()
        }
        .navigationTitle("Skins")
        .alert("Buy Skin", isPresented: .constant(confirmSkin != nil)) {
            Button("Cancel", role: .cancel) { confirmSkin = nil }
            Button("Buy & Equip") {
                if let skin = confirmSkin, store.buySkin(skin.id) {
                    store.equipSkin(skin.id)
                }
                confirmSkin = nil
            }
            .disabled(store.player.coinCount < (confirmSkin?.coinPrice ?? Int.max))
        } message: {
            if let skin = confirmSkin {
                Text("\(skin.name) costs \(skin.coinPrice) 🪙\nBalance: \(store.player.coinCount) 🪙")
            }
        }
    }
}
