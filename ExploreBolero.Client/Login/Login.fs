namespace BoleroApp.Client.Login

type RemoteService =
    {
        signIn : string * string -> Async<option<string>>
        getUsername : unit -> Async<string> // Get the user's name, or None if they are not authenticated.
        signOut : unit -> Async<unit>
    }

    interface Bolero.Remoting.IRemoteService with
        member this.BasePath = "/login"

type Message =
    | SetUsername of string
    | SetPassword of string
    | ClearFields
    | GetSignedInAs
    | RecvSignedInAs of option<string>
    | SendSignIn
    | RecvSignIn of option<string>
    | SendSignOut
    | RecvSignOut
    | Error of exn

type Model =
    {
        username: string
        password: string
        signedInAs: option<string>
        signInFailed: bool
    }

module Model =

    open Bolero.Remoting
    open Bolero.Remoting.Client
    open Elmish

    let init =
        {
            username = ""
            password = ""
            signedInAs = None
            signInFailed = false
        }

    let initCmd = Cmd.ofMsg GetSignedInAs

    let update (remote: RemoteService) (message: Message) (model: Model) =
        match message with
        | SetUsername s ->
            { model with username = s }, Cmd.none
        | SetPassword s ->
            { model with password = s }, Cmd.none
        | ClearFields ->
            { model with username = ""; password = "" }, Cmd.none
        | GetSignedInAs ->
            model, Cmd.ofRemote remote.getUsername () (fun r -> RecvSignedInAs (r.TryGetResponse())) Error
        | RecvSignedInAs username ->
            { model with signedInAs = username }, Cmd.none
        | SendSignIn ->
            model, Cmd.ofAsync remote.signIn (model.username, model.password) RecvSignIn Error
        | RecvSignIn username ->
            let isSuccess = Option.isSome username
            { model with signInFailed = not isSuccess }, Cmd.batch [
                Cmd.ofMsg (RecvSignedInAs username)
                (if isSuccess then Cmd.ofMsg ClearFields else Cmd.none)
            ]
        | SendSignOut ->
            model, Cmd.ofAsync remote.signOut () (fun () -> RecvSignOut) Error
        | RecvSignOut ->
            { model with signedInAs = None; signInFailed = false }, Cmd.none
        | Error RemoteUnauthorizedException ->
            { model with signedInAs = None }, Cmd.none
        | Error _ ->
            model, Cmd.none

module View =

    open Bolero
    open Bolero.Html
    open BoleroApp.Client
    open Elmish

    type Tmpl = Template<"Login/login.html">

    let page (model: Model) (dispatch: Dispatch<Message>) =
        Tmpl()
            .Username(model.username, fun s -> dispatch (SetUsername s))
            .Password(model.password, fun s -> dispatch (SetPassword s))
            .SignIn(fun _ -> dispatch SendSignIn)
            .ErrorNotification(
                cond model.signInFailed <| function
                | false -> empty
                | true ->
                    Common.View.Tmpl.ErrorNotification()
                        .HideClass("is-hidden")
                        .Text("Sign in failed. Use any username and the password \"password\".")
                        .Elt()
            )
            .Elt()

    let logoutButton (model: Model) (dispatch: Dispatch<Message>) =
        cond model.signedInAs <| function
        | None -> empty
        | Some loggedInAs ->
            Tmpl.LogoutButton()
                .SignOut(fun _ -> dispatch SendSignOut)
                .Username(loggedInAs)
                .Elt()
