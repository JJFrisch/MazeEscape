import Foundation

// MARK: - Seeded Random (Mulberry32)

class SeededRandom {
    private var state: UInt32

    init(seed: Int) {
        self.state = UInt32(truncatingIfNeeded: seed)
    }

    func next() -> Double {
        state &+= 0x6D2B79F5
        var t = state
        t = (t ^ (t >> 15)) &* (t | 1)
        t ^= t &+ (t ^ (t >> 7)) &* (t | 61)
        let result = t ^ (t >> 14)
        return Double(result) / Double(UInt32.max)
    }

    func nextInt(_ min: Int, _ maxExclusive: Int) -> Int {
        return min + Int(next() * Double(maxExclusive - min))
    }

    func shuffle<T>(_ array: inout [T]) {
        for i in stride(from: array.count - 1, through: 1, by: -1) {
            let j = nextInt(0, i + 1)
            array.swapAt(i, j)
        }
    }
}

// MARK: - Date Seed

func dateSeed(_ dateString: String) -> Int {
    var hash = 0
    for char in dateString.unicodeScalars {
        hash = ((hash << 5) &- hash) &+ Int(char.value)
        hash = hash & hash // Convert to 32-bit
    }
    return abs(hash)
}
