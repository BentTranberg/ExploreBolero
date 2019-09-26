namespace ExploreBolero.Server.RemoteServices

open System
open Bolero.Remoting.Server
open ExploreBolero.Client

module LoginService =

    // This remote service doesn't require any injected dependencies,
    // so it can be defined as a top-level value.
    let service (ctx: IRemoteContext) : Login.RemoteService =
        {
            signIn = fun (username, password) -> async {
                if password = "password" then
                    do! ctx.HttpContext.AsyncSignIn(username, TimeSpan.FromDays(365.))
                    return Some username
                else
                    return None
            }

            signOut = fun () -> async {
                return! ctx.HttpContext.AsyncSignOut()
            }

            getUsername = fun () -> async {
                return ctx.HttpContext.User.Identity.Name
            }
        }
