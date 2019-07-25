module BoleroApp.Client.Main.Update

open Bolero.Remoting
open BoleroApp.Client
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
        let counterModel = Counter.Update.update msg model.counter
        { model with counter = counterModel }, Cmd.none

    | Books msg ->
        let booksModel, booksCmd = Books.Update.update remote.books msg model.books
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
