import SwiftUI

struct GameplayView: View {
    @EnvironmentObject var store: GameStore
    @StateObject private var session: GameSession
    @State private var showVictory = false
    @State private var coinsEarned = 0
    @Environment(\.dismiss) private var dismiss

    let worldId: Int
    let level: CampaignLevel

    init(worldId: Int, level: CampaignLevel) {
        self.worldId = worldId
        self.level = level
        let algo = MazeAlgorithm(rawValue: level.levelType) ?? .backtracking
        let seed = worldId * 10000 + (Int(level.levelNumber) ?? 0) * 100
        _session = StateObject(wrappedValue: GameSession(
            width: level.width,
            height: level.height,
            algorithm: algo,
            seed: seed,
            twoStarMoves: level.twoStarMoves,
            threeStarTime: level.threeStarTime
        ))
    }

    var body: some View {
        VStack(spacing: 12) {
            // HUD
            HStack {
                VStack(alignment: .leading) {
                    Text("⏱️ \(String(format: "%.1f", session.elapsed))s")
                        .font(.caption.monospacedDigit())
                        .foregroundStyle(session.elapsed > Double(level.threeStarTime) ? .red : .green)
                    Text("/ \(level.threeStarTime)s")
                        .font(.caption2)
                        .foregroundStyle(.secondary)
                }
                Spacer()
                VStack(alignment: .trailing) {
                    Text("👣 \(session.moves)")
                        .font(.caption.monospacedDigit())
                        .foregroundStyle(session.moves > level.twoStarMoves ? .red : .green)
                    Text("/ \(level.twoStarMoves)")
                        .font(.caption2)
                        .foregroundStyle(.secondary)
                }
            }
            .padding(.horizontal)

            // Maze Canvas
            MazeCanvasView(session: session)
                .aspectRatio(CGFloat(session.maze.width) / CGFloat(session.maze.height), contentMode: .fit)
                .padding(.horizontal, 8)
                .gesture(
                    DragGesture(minimumDistance: 20)
                        .onEnded { value in
                            let dx = value.translation.width
                            let dy = value.translation.height
                            let dir: Direction
                            if abs(dx) > abs(dy) {
                                dir = dx > 0 ? .right : .left
                            } else {
                                dir = dy > 0 ? .down : .up
                            }
                            if session.tryMove(dir) && session.isComplete {
                                onComplete()
                            }
                        }
                )

            // Controls
            HStack(spacing: 24) {
                Button {
                    if store.usePowerup(.hint) {
                        session.getHint()
                    }
                } label: {
                    Label("Hint (\(store.player.hintsOwned))", systemImage: "lightbulb.fill")
                        .font(.caption)
                }
                .disabled(store.player.hintsOwned <= 0)

                Spacer()

                // D-pad
                VStack(spacing: 2) {
                    dpadButton(.up, icon: "chevron.up")
                    HStack(spacing: 2) {
                        dpadButton(.left, icon: "chevron.left")
                        dpadButton(.right, icon: "chevron.right")
                    }
                    dpadButton(.down, icon: "chevron.down")
                }
            }
            .padding(.horizontal)
        }
        .navigationTitle("Level \(level.levelNumber)")
        .navigationBarTitleDisplayMode(.inline)
        .onAppear { session.startTimer() }
        .onDisappear { session.stopTimer() }
        .sheet(isPresented: $showVictory) {
            VictorySheet(
                stars: session.calculateStars(),
                moves: session.moves,
                time: session.elapsed,
                level: level,
                coins: coinsEarned,
                onDismiss: { dismiss() }
            )
        }
    }

    private func dpadButton(_ dir: Direction, icon: String) -> some View {
        Button {
            if session.tryMove(dir) && session.isComplete {
                onComplete()
            }
        } label: {
            Image(systemName: icon)
                .frame(width: 44, height: 44)
                .background(.ultraThinMaterial, in: RoundedRectangle(cornerRadius: 10))
        }
    }

    private func onComplete() {
        let stars = session.calculateStars()
        coinsEarned = 50 + stars.total * 25
        store.addCoins(coinsEarned)

        var updated = level
        updated.completed = true
        updated.star1 = stars.star1
        updated.star2 = stars.star2
        updated.star3 = stars.star3
        updated.numberOfStars = stars.total
        updated.bestMoves = session.moves
        updated.bestTime = session.elapsed
        store.saveLevelProgress(worldId: worldId, level: updated)

        showVictory = true
    }
}

// MARK: - Victory Sheet

private struct VictorySheet: View {
    let stars: (star1: Bool, star2: Bool, star3: Bool, total: Int)
    let moves: Int
    let time: Double
    let level: CampaignLevel
    let coins: Int
    let onDismiss: () -> Void

    var body: some View {
        VStack(spacing: 24) {
            Text("🎉 Level Complete!")
                .font(.title.bold())

            HStack(spacing: 8) {
                ForEach(0..<3, id: \.self) { i in
                    Image(systemName: i < stars.total ? "star.fill" : "star")
                        .font(.title)
                        .foregroundStyle(i < stars.total ? .yellow : .secondary)
                }
            }

            VStack(spacing: 8) {
                HStack {
                    Text("Time")
                        .foregroundStyle(.secondary)
                    Spacer()
                    Text("\(String(format: "%.1f", time))s / \(level.threeStarTime)s")
                }
                HStack {
                    Text("Moves")
                        .foregroundStyle(.secondary)
                    Spacer()
                    Text("\(moves) / \(level.twoStarMoves)")
                }
                HStack {
                    Text("Coins")
                        .foregroundStyle(.secondary)
                    Spacer()
                    Text("+\(coins) 🪙")
                        .foregroundStyle(.yellow)
                        .bold()
                }
            }
            .font(.subheadline)
            .padding()
            .background(.ultraThinMaterial, in: RoundedRectangle(cornerRadius: 12))

            Button("Done") { onDismiss() }
                .buttonStyle(.borderedProminent)
                .tint(.indigo)
        }
        .padding(32)
        .presentationDetents([.medium])
    }
}
