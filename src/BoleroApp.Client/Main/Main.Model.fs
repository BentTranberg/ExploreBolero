namespace BoleroApp.Client.Main

open BoleroApp.Client

open Bolero

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/counter">] Counter
    | [<EndPoint "/data">] Data

/// The Elmish application's model.
type Model =
    {
        page: Page
        error: string option
        counter: Counter.Model
        books: Books.Model
        login: Login.Model
    }

module Model =

    let init =
        {
            page = Home
            error = None
            counter = Counter.Model.init
            books = Books.Model.init
            login = Login.Model.init
        }

type RemoteServices =
    {
        books: Books.RemoteService
        login: Login.RemoteService
    }

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | Error of exn
    | Counter of Counter.Message
    | Books of Books.Message
    | Login of Login.Message
    | ClearError

module Router =

    /// Connects the routing system to the Elmish application.
    let router = Router.infer SetPage (fun model -> model.page)
