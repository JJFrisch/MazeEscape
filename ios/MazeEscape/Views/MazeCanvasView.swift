import SwiftUI

/// Renders a maze using Canvas (available iOS 15+).
struct MazeCanvasView: View {
    @ObservedObject var session: GameSession

    var body: some View {
        Canvas { context, size in
            let maze = session.maze
            let cellW = size.width / CGFloat(maze.width)
            let cellH = size.height / CGFloat(maze.height)

            // Background
            context.fill(Path(CGRect(origin: .zero, size: size)), with: .color(.black.opacity(0.9)))

            // Start cell
            let startRect = CGRect(
                x: CGFloat(maze.start.x) * cellW,
                y: CGFloat(maze.start.y) * cellH,
                width: cellW,
                height: cellH
            )
            context.fill(Path(startRect), with: .color(.indigo.opacity(0.3)))

            // End cell
            let endRect = CGRect(
                x: CGFloat(maze.end.x) * cellW,
                y: CGFloat(maze.end.y) * cellH,
                width: cellW,
                height: cellH
            )
            context.fill(Path(endRect), with: .color(.green.opacity(0.3)))

            // Hint path
            if let hintPath = session.hintPath, hintPath.count > 1 {
                var hintLine = Path()
                for (i, pos) in hintPath.enumerated() {
                    let point = CGPoint(
                        x: CGFloat(pos.x) * cellW + cellW / 2,
                        y: CGFloat(pos.y) * cellH + cellH / 2
                    )
                    if i == 0 { hintLine.move(to: point) }
                    else { hintLine.addLine(to: point) }
                }
                context.stroke(hintLine, with: .color(.yellow.opacity(0.5)), lineWidth: 2)
            }

            // Walls
            let wallColor: Color = .white
            let wallWidth: CGFloat = 2

            for y in 0..<maze.height {
                for x in 0..<maze.width {
                    let cell = maze.cells[y][x]
                    let cx = CGFloat(x) * cellW
                    let cy = CGFloat(y) * cellH

                    // North wall
                    if cell.north {
                        var p = Path()
                        p.move(to: CGPoint(x: cx, y: cy))
                        p.addLine(to: CGPoint(x: cx + cellW, y: cy))
                        context.stroke(p, with: .color(wallColor), lineWidth: wallWidth)
                    }

                    // East wall
                    if cell.east {
                        var p = Path()
                        p.move(to: CGPoint(x: cx + cellW, y: cy))
                        p.addLine(to: CGPoint(x: cx + cellW, y: cy + cellH))
                        context.stroke(p, with: .color(wallColor), lineWidth: wallWidth)
                    }
                }
            }

            // Border walls
            var border = Path()
            // Top
            border.move(to: .zero)
            border.addLine(to: CGPoint(x: size.width, y: 0))
            // Right
            border.addLine(to: CGPoint(x: size.width, y: size.height))
            // Bottom
            border.addLine(to: CGPoint(x: 0, y: size.height))
            // Left
            border.addLine(to: .zero)
            context.stroke(border, with: .color(wallColor), lineWidth: wallWidth)

            // Player
            let px = CGFloat(session.playerPos.x) * cellW + cellW / 2
            let py = CGFloat(session.playerPos.y) * cellH + cellH / 2
            let radius = min(cellW, cellH) * 0.35

            // Glow
            context.fill(
                Path(ellipseIn: CGRect(x: px - radius * 1.5, y: py - radius * 1.5, width: radius * 3, height: radius * 3)),
                with: .color(.indigo.opacity(0.2))
            )

            // Player circle
            context.fill(
                Path(ellipseIn: CGRect(x: px - radius, y: py - radius, width: radius * 2, height: radius * 2)),
                with: .color(.indigo)
            )

            // Goal marker
            let gx = CGFloat(maze.end.x) * cellW + cellW / 2
            let gy = CGFloat(maze.end.y) * cellH + cellH / 2
            context.fill(
                Path(ellipseIn: CGRect(x: gx - radius * 0.6, y: gy - radius * 0.6, width: radius * 1.2, height: radius * 1.2)),
                with: .color(.green)
            )
        }
    }
}
