module BoleroApp.Client.Counter.Update

let update (message: Message) (model: Model) =
    match message with
    | SetValue v ->
        { model with value = v }
    | Increment ->
        { model with value = model.value + 1 }
    | Decrement ->
        { model with value = model.value - 1 }
