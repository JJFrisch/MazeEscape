import SwiftUI

struct SettingsView: View {
    @EnvironmentObject var store: GameStore
    @State private var nameInput: String = ""
    @State private var showSaved = false

    let presetColors: [String] = [
        "#6366F1", "#8B5CF6", "#EC4899", "#EF4444",
        "#F59E0B", "#10B981", "#06B6D4", "#FFFFFF"
    ]

    var body: some View {
        Form {
            Section("Player Name") {
                HStack {
                    TextField("Name", text: $nameInput)
                        .onAppear { nameInput = store.player.playerName }
                    Button("Save") {
                        store.player.playerName = nameInput.trimmingCharacters(in: .whitespaces)
                        flash()
                    }
                }
            }

            Section("Wall Color") {
                LazyVGrid(columns: [GridItem(.adaptive(minimum: 40))], spacing: 8) {
                    ForEach(presetColors, id: \.self) { hex in
                        Circle()
                            .fill(Color(hex: hex))
                            .frame(width: 36, height: 36)
                            .overlay(
                                Circle()
                                    .stroke(.white, lineWidth: store.player.wallColor == hex ? 3 : 0)
                            )
                            .onTapGesture {
                                store.player.wallColor = hex
                                flash()
                            }
                    }
                }
            }

            Section("Stats") {
                HStack {
                    Text("Coins")
                    Spacer()
                    Text("\(store.player.coinCount)")
                        .foregroundStyle(.secondary)
                }
                HStack {
                    Text("Skins Owned")
                    Spacer()
                    Text("\(store.player.unlockedSkinIds.count)")
                        .foregroundStyle(.secondary)
                }
            }

            Section("About") {
                Text("MazeEscape")
                    .font(.headline)
                Text("A maze puzzle game")
                    .foregroundStyle(.secondary)
            }
        }
        .navigationTitle("Settings")
        .overlay(alignment: .top) {
            if showSaved {
                Text("Saved!")
                    .font(.caption.bold())
                    .foregroundStyle(.black)
                    .padding(.horizontal, 16)
                    .padding(.vertical, 8)
                    .background(.green, in: Capsule())
                    .transition(.move(edge: .top).combined(with: .opacity))
                    .padding(.top, 8)
            }
        }
    }

    private func flash() {
        withAnimation { showSaved = true }
        DispatchQueue.main.asyncAfter(deadline: .now() + 1.5) {
            withAnimation { showSaved = false }
        }
    }
}

// MARK: - Color from Hex

extension Color {
    init(hex: String) {
        let hex = hex.trimmingCharacters(in: .init(charactersIn: "#"))
        let scanner = Scanner(string: hex)
        var rgb: UInt64 = 0
        scanner.scanHexInt64(&rgb)
        self.init(
            red: Double((rgb >> 16) & 0xFF) / 255,
            green: Double((rgb >> 8) & 0xFF) / 255,
            blue: Double(rgb & 0xFF) / 255
        )
    }
}
