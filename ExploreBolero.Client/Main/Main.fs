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
    | [<EndPoint "/blazordates">] BlazorDates
    | [<EndPoint "/dates">] Dates
    | [<EndPoint "/altlogin">] AltLogin

type PageModel =
    | NoPageModel
    | CounterModel of Counter.Model
    | BulmaExtModel of BulmaExt.Model
    | BlazorDatesModel of BlazorDates.Model
    | DatesModel of Dates.Model
    | BooksModel of Books.Model

type Message =
    | SetPage of Page
    | ToggleBurger
    | Error of exn
    | Counter of Counter.Message
    | BulmaExt of BulmaExt.Message
    | BlazorDates of BlazorDates.Message
    | Dates of Dates.Message
    | Books of Books.Message
    | Login of Login.Message
    | ClearError

type Model =
    {
        page: Page
        pageModel: PageModel
        error: string option
        navBarBurgerActive: bool
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
            navBarBurgerActive = false
            pageModel = NoPageModel
            books = Books.Model.init
            login = Login.Model.init
        }

    let initCmd = Cmd.batch [
        Cmd.map Login Login.Model.initCmd
    ]

    let update (remote: RemoteServices) (message: Message) (model: Model) =
        match message with
        | ToggleBurger ->
            { model with navBarBurgerActive = not model.navBarBurgerActive }, Cmd.none
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
            let pageModel =
                match page with
                | Page.Counter -> CounterModel Counter.Model.init
                | Page.BulmaExt -> BulmaExtModel BulmaExt.Model.init
                | Page.BlazorDates -> BlazorDatesModel BlazorDates.Model.init
                | Page.Dates -> DatesModel Dates.Model.init
                | _ -> NoPageModel
            { model with page = page; pageModel = pageModel }, Cmd.none

        | Counter msg ->
            match model.pageModel with
            | CounterModel counterModel ->
                let counterModel' = Counter.Model.update msg counterModel
                { model with pageModel = CounterModel counterModel' }, Cmd.none
            | _ -> model, Cmd.none

        | BulmaExt msg ->
            match model.pageModel with
            | BulmaExtModel bulmaExtModel ->
                let bulmaExtModel', bulmaExtCmd' = BulmaExt.Model.update msg bulmaExtModel
                { model with pageModel = BulmaExtModel bulmaExtModel' }, Cmd.map BulmaExt bulmaExtCmd'
            | _ -> model, Cmd.none

        | BlazorDates msg ->
            match model.pageModel with
            | BlazorDatesModel blazorDatesModel ->
                let blazorDatesModel', blazorDatesCmd' = BlazorDates.Model.update msg blazorDatesModel
                { model with pageModel = BlazorDatesModel blazorDatesModel' }, Cmd.map Dates blazorDatesCmd'
            | _ -> model, Cmd.none

        | Dates msg ->
            match model.pageModel with
            | DatesModel datesModel ->
                let datesModel', datesCmd' = Dates.Model.update msg datesModel
                { model with pageModel = DatesModel datesModel' }, Cmd.map Dates datesCmd'
            | _ -> model, Cmd.none

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

    let noPage = text "No page."

    let view (model: Model) (dispatch: Dispatch<Message>) =
        Tmpl()
            .NavBarBurgerClick(fun _ -> dispatch ToggleBurger)
            .NavBarBurgerActive(match model.navBarBurgerActive with true -> "is-active" | false -> "")
            .NavBarMenuActive(match model.navBarBurgerActive with true -> "is-active" | false -> "")
            .Menu(concat [
                menuItem model Page.Home "Home"
                menuItem model Page.Counter "Counter"
                menuItem model Page.Data "Download data"
                menuItem model Page.BulmaExt "Bulma Extensions"
                menuItem model Page.BlazorDates "Blazor Dates"
                menuItem model Page.Dates "Bulma Ext. Dates"
                menuItem model Page.AltLogin "Bulma Alt. Login"
                Login.View.logoutButton model.login (dispatch << Message.Login)
            ])
            .Body(
                cond model.page <| function
                | Page.Home -> Home.View.homePage ()
                | Page.Counter ->
                    match model.pageModel with
                    | CounterModel counterModel ->
                        Counter.View.page counterModel (dispatch << Message.Counter)
                    | _ -> noPage
                | Page.BulmaExt ->
                    match model.pageModel with
                    | BulmaExtModel bulmaExtModel ->
                        BulmaExt.View.page bulmaExtModel (dispatch << Message.BulmaExt)
                    | _ -> noPage
                | Page.BlazorDates ->
                    match model.pageModel with
                    | BlazorDatesModel blazorDatesModel ->
                        BlazorDates.View.page blazorDatesModel (dispatch << Message.BlazorDates)
                    | _ -> noPage
                | Page.Dates ->
                    match model.pageModel with
                    | DatesModel datesModel ->
                        Dates.View.page datesModel (dispatch << Message.Dates)
                    | _ -> noPage
                | Page.Data ->
                    cond model.login.signedInAs <| function
                    | Some _ -> Books.View.page model.books (dispatch << Message.Books)
                    | None -> Login.View.page model.login (dispatch << Message.Login)
                | Page.AltLogin -> AltLogin.View.altLoginPage ()
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
