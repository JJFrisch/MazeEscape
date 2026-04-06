import SwiftUI

struct DailyMazeView: View {
    @EnvironmentObject var store: GameStore
    @State private var selectedDate: Date?
    @State private var showGame = false

    private let calendar = Calendar.current
    private let today = Date()

    var body: some View {
        Group {
            if !store.isDailyUnlocked() {
                lockedView
            } else {
                calendarView
            }
        }
        .navigationTitle("Daily Maze")
        .sheet(item: $selectedDate) { date in
            NavigationStack {
                DailyGameView(date: date)
                    .environmentObject(store)
            }
        }
    }

    private var lockedView: some View {
        VStack(spacing: 16) {
            Text("🔒")
                .font(.system(size: 60))
            Text("Complete World 1, Level 10 to unlock!")
                .foregroundStyle(.secondary)
                .multilineTextAlignment(.center)
        }
        .padding()
    }

    private var calendarView: some View {
        ScrollView {
            LazyVGrid(columns: Array(repeating: GridItem(.flexible()), count: 7), spacing: 8) {
                ForEach(["S","M","T","W","T","F","S"], id: \.self) { day in
                    Text(day)
                        .font(.caption2.bold())
                        .foregroundStyle(.secondary)
                }

                ForEach(daysInMonth(), id: \.self) { date in
                    let day = calendar.component(.day, from: date)
                    let shortDate = formatShortDate(date)
                    let result = store.getDailyResult(shortDate)
                    let isFuture = date > today

                    Button {
                        if !isFuture { selectedDate = date }
                    } label: {
                        VStack(spacing: 2) {
                            Text("\(day)")
                                .font(.caption.bold())
                            if result?.completed == true {
                                Text("✓")
                                    .font(.caption2)
                                    .foregroundStyle(.green)
                            }
                        }
                        .frame(maxWidth: .infinity)
                        .frame(height: 44)
                        .background(
                            calendar.isDateInToday(date)
                                ? Color.indigo.opacity(0.2)
                                : result?.completed == true
                                    ? Color.green.opacity(0.1)
                                    : Color(uiColor: .secondarySystemBackground)
                        )
                        .clipShape(RoundedRectangle(cornerRadius: 8))
                        .opacity(isFuture ? 0.3 : 1)
                    }
                    .disabled(isFuture)
                }
            }
            .padding()
        }
    }

    private func daysInMonth() -> [Date] {
        let components = calendar.dateComponents([.year, .month], from: today)
        guard let startOfMonth = calendar.date(from: components),
              let range = calendar.range(of: .day, in: .month, for: startOfMonth) else { return [] }
        return range.compactMap { calendar.date(byAdding: .day, value: $0 - 1, to: startOfMonth) }
    }

    private func formatShortDate(_ date: Date) -> String {
        let c = calendar.dateComponents([.month, .day, .year], from: date)
        return "\(c.month!)/\(c.day!)/\(c.year!)"
    }
}

// Make Date conform to Identifiable for sheet
extension Date: @retroactive Identifiable {
    public var id: TimeInterval { timeIntervalSince1970 }
}

// MARK: - Daily Game View

struct DailyGameView: View {
    @EnvironmentObject var store: GameStore
    @StateObject private var session: GameSession
    @State private var showVictory = false
    @State private var coinsEarned = 0
    @Environment(\.dismiss) private var dismiss

    let date: Date
    private let shortDate: String

    init(date: Date) {
        self.date = date
        let cal = Calendar.current
        let c = cal.dateComponents([.month, .day, .year], from: date)
        self.shortDate = "\(c.month!)/\(c.day!)/\(c.year!)"

        let seed = dateSeed(self.shortDate)
        let rng = SeededRandom(seed: seed)
        let dayOfMonth = cal.component(.day, from: date)
        let difficulty = min(Double(dayOfMonth) / 31.0, 1.0)
        let minSize = 4 + Int(difficulty * 6)
        let maxSize = minSize + 3
        let width = rng.nextInt(minSize, maxSize + 1)
        let height = rng.nextInt(minSize, maxSize + 1)
        let algorithms: [MazeAlgorithm] = MazeAlgorithm.allCases
        let algorithm = algorithms[rng.nextInt(0, algorithms.count)]
        let area = width * height
        let movesNeeded = Int(Double(area) * 1.8)
        let timeNeeded = Int(Double(area) * 0.6) + 10

        _session = StateObject(wrappedValue: GameSession(
            width: width,
            height: height,
            algorithm: algorithm,
            seed: seed,
            twoStarMoves: movesNeeded,
            threeStarTime: timeNeeded
        ))
    }

    var body: some View {
        VStack(spacing: 12) {
            HStack {
                Text("⏱️ \(String(format: "%.1f", session.elapsed))s")
                    .font(.caption.monospacedDigit())
                Spacer()
                Text("👣 \(session.moves)")
                    .font(.caption.monospacedDigit())
            }
            .padding(.horizontal)

            MazeCanvasView(session: session)
                .aspectRatio(CGFloat(session.maze.width) / CGFloat(session.maze.height), contentMode: .fit)
                .padding(.horizontal, 8)
                .gesture(
                    DragGesture(minimumDistance: 20)
                        .onEnded { value in
                            let dx = value.translation.width
                            let dy = value.translation.height
                            let dir: Direction = abs(dx) > abs(dy)
                                ? (dx > 0 ? .right : .left)
                                : (dy > 0 ? .down : .up)
                            if session.tryMove(dir) && session.isComplete {
                                onComplete()
                            }
                        }
                )

            HStack {
                Button {
                    if store.usePowerup(.hint) { session.getHint() }
                } label: {
                    Label("Hint (\(store.player.hintsOwned))", systemImage: "lightbulb.fill")
                        .font(.caption)
                }
                .disabled(store.player.hintsOwned <= 0)
                Spacer()
            }
            .padding(.horizontal)
        }
        .navigationTitle("📅 Daily Maze")
        .navigationBarTitleDisplayMode(.inline)
        .toolbar {
            ToolbarItem(placement: .cancellationAction) {
                Button("Close") { dismiss() }
            }
        }
        .onAppear { session.startTimer() }
        .onDisappear { session.stopTimer() }
        .alert("🎉 Complete!", isPresented: $showVictory) {
            Button("Done") { dismiss() }
        } message: {
            Text("⏱️ \(String(format: "%.1f", session.elapsed))s  👣 \(session.moves) moves\n+\(coinsEarned) 🪙")
        }
    }

    private func onComplete() {
        let stars = session.calculateStars()
        coinsEarned = 50 + stars.total * 25
        store.addCoins(coinsEarned)
        store.saveDailyResult(DailyMazeResult(
            shortDate: shortDate,
            completed: true,
            completionTime: session.elapsed,
            completionMoves: session.moves
        ))
        showVictory = true
    }
}
