// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

namespace Pretwa

type public FieldState =
    | Empty
    | Black
    | Red

type public BoardState = {
    Edge : FieldState[,]
    Center: FieldState
}

type public Move = {
    Before : BoardState
    After : BoardState
}