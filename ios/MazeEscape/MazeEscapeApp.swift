import SwiftUI

@main
struct MazeEscapeApp: App {
    @StateObject private var gameStore = GameStore()

    var body: some Scene {
        WindowGroup {
            ContentView()
                .environmentObject(gameStore)
                .preferredColorScheme(.dark)
        }
    }
}
