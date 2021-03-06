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
    | [<EndPoint "/counter">] Counter of PageModel<Counter.Model>
    | [<EndPoint "/dropdown">] Dropdown of PageModel<Dropdown.Model>
    | [<EndPoint "/data">] Data
    | [<EndPoint "/bulmaext">] BulmaExt of PageModel<BulmaExt.Model>
    | [<EndPoint "/blazordates">] BlazorDates of PageModel<BlazorDates.Model>
    | [<EndPoint "/dates">] Dates of PageModel<Dates.Model>
    | [<EndPoint "/altlogin">] AltLogin

type Message =
    | SetPage of Page
    | ToggleBurger
    | Error of exn
    | Counter of Counter.Message
    | Dropdown of Dropdown.Message
    | BulmaExt of BulmaExt.Message
    | BlazorDates of BlazorDates.Message
    | Dates of Dates.Message
    | Books of Books.Message
    | Login of Login.Message
    | ClearError

type Model =
    {
        page: Page
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
            { model with page = page }, Cmd.none

        | Counter msg ->
            match model.page with
            | Page.Counter x ->
                let m' = Counter.Model.update msg x.Model
                { model with page = Page.Counter { Model = m' } }, Cmd.none
            | _ -> model, Cmd.none

        | Dropdown msg ->
            match model.page with
            | Page.Dropdown x ->
                let m', cmd' = Dropdown.Model.update msg x.Model
                { model with page = Page.Dropdown { Model = m' } }, Cmd.map Dropdown cmd'
            | _ -> model, Cmd.none

        | BulmaExt msg ->
            match model.page with
            | Page.BulmaExt x ->
                let m', cmd' = BulmaExt.Model.update msg x.Model
                { model with page = Page.BulmaExt { Model = m' } }, Cmd.map BulmaExt cmd'
            | _ -> model, Cmd.none

        | BlazorDates msg ->
            match model.page with
            | Page.BlazorDates x ->
                let m', cmd' = BlazorDates.Model.update msg x.Model
                { model with page = Page.BlazorDates { Model = m' } }, Cmd.map BlazorDates cmd'
            | _ -> model, Cmd.none

        | Dates msg ->
            match model.page with
            | Page.Dates x ->
                let m', cmd' = Dates.Model.update msg x.Model
                { model with page = Page.Dates { Model = m' } }, Cmd.map Dates cmd'
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

    open System

    let defaultModel = function
        | Page.Counter model -> Router.definePageModel model Counter.Model.init
        | Page.Dropdown model -> Router.definePageModel model Dropdown.Model.init
        | Page.BulmaExt model -> Router.definePageModel model BulmaExt.Model.init
        | Page.BlazorDates model -> Router.definePageModel model BlazorDates.Model.init
        | Page.Dates model -> Router.definePageModel model Dates.Model.init
        | _ -> ()

    let router = Router.inferWithModel SetPage (fun model -> model.page) defaultModel

module View =

    open Bolero.Html
    open Elmish

    type Tmpl = Template<"Main/main.html">

    let menuItem (model: Model) (page: Page) (text: string) =
        Tmpl.MenuItem()
            .Active(
                match model.page, page with
                | Page.Home, Page.Home
                | Page.Counter _, Page.Counter _
                | Page.Dropdown _, Page.Dropdown _
                | Page.Data, Page.Data
                | Page.BulmaExt _, Page.BulmaExt _
                | Page.BlazorDates _, Page.BlazorDates _
                | Page.Dates _, Page.Dates _
                | Page.AltLogin _, Page.AltLogin _
                    -> "is-active"
                | _ -> ""
                )
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
                menuItem model (Page.Counter Router.noModel) "Counter"
                menuItem model (Page.Dropdown Router.noModel) "Dropdown"
                menuItem model Page.Data "Download data"
                menuItem model (Page.BulmaExt Router.noModel) "Bulma Extensions"
                menuItem model (Page.BlazorDates Router.noModel) "Blazor Dates"
                menuItem model (Page.Dates Router.noModel) "Bulma Ext. Dates"
                menuItem model Page.AltLogin "Bulma Alt. Login"
                Login.View.logoutButton model.login (dispatch << Message.Login)
            ])
            .Body(
                cond model.page <| function
                | Page.Home -> Home.View.homePage ()
                | Page.Counter x -> Counter.View.page x.Model (dispatch << Message.Counter)
                | Page.Dropdown x -> Dropdown.View.page x.Model (dispatch << Message.Dropdown)
                | Page.BulmaExt x -> BulmaExt.View.page x.Model (dispatch << Message.BulmaExt)
                | Page.BlazorDates x -> BlazorDates.View.page x.Model (dispatch << Message.BlazorDates)
                | Page.Dates x -> Dates.View.page x.Model (dispatch << Message.Dates)
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
