namespace BoleroApp.Client.Main

open BoleroApp.Client
open Bolero

type RemoteServices =
    {
        books: Books.RemoteService
        login: Login.RemoteService
    }

type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/counter">] Counter
    | [<EndPoint "/data">] Data

type Message =
    | SetPage of Page
    | Error of exn
    | Counter of Counter.Message
    | Books of Books.Message
    | Login of Login.Message
    | ClearError

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

module Update =

    open Bolero.Remoting
    open Elmish

    let update (remote: RemoteServices) (message: Message) (model: Model) =
        match message with
        | Error exn
        | Books (Books.Error exn)
        | Login (Login.Error exn) ->
            match exn with
            | RemoteUnauthorizedException ->
                { model with error = Some "You have been logged out." }, Cmd.none
            | exn ->
                { model with error = Some exn.Message }, Cmd.none
        | ClearError ->
            { model with error = None }, Cmd.none

        | SetPage page ->
            { model with page = page }, Cmd.none

        | Counter msg ->
            let counterModel = Counter.Model.update msg model.counter
            { model with counter = counterModel }, Cmd.none

        | Books msg ->
            let booksModel, booksCmd = Books.Model.update remote.books msg model.books
            { model with books = booksModel }, Cmd.map Books booksCmd

        | Login msg ->
            let loginModel, loginCmd = Login.Update.update remote.login msg model.login
            let model = { model with login = loginModel }
            let cmd = Cmd.map Login loginCmd
            let extraCmd =
                match msg with
                | Login.RecvSignedInAs (Some _) -> Cmd.ofMsg (Books Books.GetBooks)
                | _ -> Cmd.none
            model, Cmd.batch [cmd; extraCmd]

    let init = Cmd.batch [
        Cmd.map Login Login.Update.init
    ]

module Router =

    let router = Router.infer SetPage (fun model -> model.page)

module View =

    open Bolero.Html
    open Elmish

    type Tmpl = Template<"Main/main.html">

    let menuItem (model: Model) (page: Page) (text: string) =
        Tmpl.MenuItem()
            .Active(if model.page = page then "is-active" else "")
            .Url(Router.router.Link page)
            .Text(text)
            .Elt()

    let homePage () =
        Tmpl.Home().Elt()

    let view (model: Model) (dispatch: Dispatch<Message>) =
        Tmpl()
            .Menu(concat [
                menuItem model Page.Home "Home"
                menuItem model Page.Counter "Counter"
                menuItem model Page.Data "Download data"
                Login.View.logoutButton model.login (dispatch << Message.Login)
            ])
            .Body(
                cond model.page <| function
                | Page.Home -> homePage ()
                | Page.Counter -> Counter.View.page model.counter (dispatch << Message.Counter)
                | Page.Data ->
                    cond model.login.signedInAs <| function
                    | Some _ -> Books.View.page model.books (dispatch << Message.Books)
                    | None -> Login.View.page model.login (dispatch << Message.Login)
            )
            .Error(
                cond model.error <| function
                | None -> empty
                | Some err ->
                    Common.View.Tmpl.ErrorNotification()
                        .Text(err)
                        .Hide(fun _ -> dispatch ClearError)
                        .Elt()
            )
            .Elt()
