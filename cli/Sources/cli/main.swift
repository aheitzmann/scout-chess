// Test moves
for notation in ["g8f6", "h7h5", "a2a4", "b1a3", "g1h3", "g8h6", "b8a6", "d7d5", "e7e6", "f2f4"] {
    let actual = Move.get(notation: notation).getNotation()
    precondition(notation == actual, "Expected: \(notation), Actual: \(actual)")
}

let board = Board()
var index = 0
while index < 10 {
    print(board)
    print("Enter move: ", terminator: "")
    let input = readLine()
    if input == nil {
        print("No input provided!")
        continue
    } else if input == "exit" {
        print("Bye!")
        break
    } else {
        let move = Move.get(notation: input!)
        if board.confirmMove(move: move) {
            print("Confirmed move.")
        } else {
            print("Invalid move.")
        }
        index += 1
    }
}
print(board)