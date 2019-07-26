namespace ExploreBolero.Client.Counter

type Message =
    | Increment
    | Decrement
    | SetValue of int

type Model =
    {
        value: int
    }

module Model =

    let init = { value = 0 }

    let update (message: Message) (model: Model) =
        match message with
        | SetValue v ->
            { model with value = v }
        | Increment ->
            { model with value = model.value + 1 }
        | Decrement ->
            { model with value = model.value - 1 }

module View =

    open Bolero
    open Elmish

    type Tmpl = Template<"Counter/counter.html">

    let page (model: Model) (dispatch: Dispatch<Message>) =
        Tmpl()
            .Decrement(fun _ -> dispatch Decrement)
            .Increment(fun _ -> dispatch Increment)
            .Value(model.value, fun v -> dispatch (SetValue v))
            .Elt()
