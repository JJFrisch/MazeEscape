import SwiftUI

struct ContentView: View {
    @EnvironmentObject var store: GameStore

    var body: some View {
        TabView {
            NavigationStack {
                LandingView()
            }
            .tabItem {
                Label("Home", systemImage: "house.fill")
            }

            NavigationStack {
                WorldsView()
            }
            .tabItem {
                Label("Campaign", systemImage: "map.fill")
            }

            NavigationStack {
                DailyMazeView()
            }
            .tabItem {
                Label("Daily", systemImage: "calendar")
            }

            NavigationStack {
                ShopView()
            }
            .tabItem {
                Label("Shop", systemImage: "cart.fill")
            }

            NavigationStack {
                SettingsView()
            }
            .tabItem {
                Label("Settings", systemImage: "gearshape.fill")
            }
        }
        .tint(.indigo)
    }
}
