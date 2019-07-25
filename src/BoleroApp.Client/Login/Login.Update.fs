module BoleroApp.Client.Login.Update

open Bolero.Remoting
open Bolero.Remoting.Client
open Elmish

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

let init =
    Cmd.ofMsg GetSignedInAs
