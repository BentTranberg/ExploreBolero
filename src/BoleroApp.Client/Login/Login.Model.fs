namespace BoleroApp.Client.Login

open Bolero.Remoting.Client
open Bolero.Remoting

type Model =
    {
        username: string
        password: string
        signedInAs: option<string>
        signInFailed: bool
    }

module Model =

    let init =
        {
            username = ""
            password = ""
            signedInAs = None
            signInFailed = false
        }

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

type RemoteService =
    {
        /// Sign into the application.
        signIn : string * string -> Async<option<string>>

        /// Get the user's name, or None if they are not authenticated.
        getUsername : unit -> Async<string>

        /// Sign out from the application.
        signOut : unit -> Async<unit>
    }

    interface IRemoteService with
        member this.BasePath = "/login"
