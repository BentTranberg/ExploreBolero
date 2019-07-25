namespace BoleroApp.Client.Counter

type Model =
    {
        value: int
    }

module Model =

    let init = { value = 0 }

type Message =
    | Increment
    | Decrement
    | SetValue of int
