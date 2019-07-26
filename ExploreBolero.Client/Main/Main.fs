namespace ExploreBolero.Client.Main

open Bolero
open ExploreBolero.Client

type RemoteServices =
    {
        books: Books.RemoteService
        login: Login.RemoteService
    }

type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/counter">] Counter
    | [<EndPoint "/data">] Data
    | [<EndPoint "/bulmaext">] BulmaExt
    | [<EndPoint "/dates">] Dates

type Message =
    | SetPage of Page
    | Error of exn
    | Counter of Counter.Message
    | BulmaExt of BulmaExt.Message
    | Dates of Dates.Message
    | Books of Books.Message
    | Login of Login.Message
    | ClearError

type Model =
    {
        page: Page
        error: string option
        counter: Counter.Model
        bulmaExt: BulmaExt.Model
        dates: Dates.Model
        books: Books.Model
        login: Login.Model
    }

module Model =

    open Bolero.Remoting
    open Elmish

    let init =
        {
            page = Home
            error = None
            counter = Counter.Model.init
            bulmaExt = BulmaExt.Model.init
            dates = Dates.Model.init
            books = Books.Model.init
            login = Login.Model.init
        }

    let initCmd = Cmd.batch [
        Cmd.map Login Login.Model.initCmd
    ]

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

        | BulmaExt msg ->
            let bulmaExtModel, bulmaExtCmd = BulmaExt.Model.update msg model.bulmaExt
            { model with bulmaExt = bulmaExtModel }, Cmd.map BulmaExt bulmaExtCmd

        | Dates msg ->
            let datesModel, datesCmd = Dates.Model.update msg model.dates
            { model with dates = datesModel }, Cmd.map Dates datesCmd

        | Books msg ->
            let booksModel, booksCmd = Books.Model.update remote.books msg model.books
            { model with books = booksModel }, Cmd.map Books booksCmd

        | Login msg ->
            let loginModel, loginCmd = Login.Model.update remote.login msg model.login
            let model = { model with login = loginModel }
            let cmd = Cmd.map Login loginCmd
            let extraCmd =
                match msg with
                | Login.RecvSignedInAs (Some _) -> Cmd.ofMsg (Books Books.GetBooks)
                | _ -> Cmd.none
            model, Cmd.batch [cmd; extraCmd]

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

    let view (model: Model) (dispatch: Dispatch<Message>) =
        Tmpl()
            .Menu(concat [
                menuItem model Page.Home "Home"
                menuItem model Page.Counter "Counter"
                menuItem model Page.BulmaExt "Bulma Extensions"
                menuItem model Page.Dates "Dates"
                menuItem model Page.Data "Download data"
                Login.View.logoutButton model.login (dispatch << Message.Login)
            ])
            .Body(
                cond model.page <| function
                | Page.Home -> Home.View.homePage ()
                | Page.Counter -> Counter.View.page model.counter (dispatch << Message.Counter)
                | Page.BulmaExt -> BulmaExt.View.page model.bulmaExt (dispatch << Message.BulmaExt)
                | Page.Dates -> Dates.View.page model.dates (dispatch << Message.Dates)
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
