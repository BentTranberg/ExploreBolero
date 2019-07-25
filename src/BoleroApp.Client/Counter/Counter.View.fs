module BoleroApp.Client.Counter.View

open Bolero
open Elmish

type Tmpl = Template<"Counter/counter.html">

let page (model: Model) (dispatch: Dispatch<Message>) =
    Tmpl()
        .Decrement(fun _ -> dispatch Decrement)
        .Increment(fun _ -> dispatch Increment)
        .Value(model.value, fun v -> dispatch (SetValue v))
        .Elt()
