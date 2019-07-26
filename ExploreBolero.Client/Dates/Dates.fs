namespace ExploreBolero.Client.Dates

type Message =
    | Dummy

type Model =
    {
        dummy: string
    }

module Model =

    open Bolero.Remoting.Client
    open Elmish

    let init = { dummy = "" }

    let update (message: Message) (model: Model) =
        match message with
        | Dummy ->
            model, Cmd.none

module View =

    open Bolero
    open Bolero.Html
    open Elmish

    type Tmpl = Template<"Dates/dates.html">

    let page (model: Model) (dispatch: Dispatch<Message>) =
        Tmpl()
            .Elt()
