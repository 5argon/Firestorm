# Firestorm 

![icon](.icon.png)

Makeshift Cloud Firestore C# API that works on Unity via REST API, by `UnityWebRequest` that can ensure cross-platform support. Only basic functions implemented. "Makeshift" means I wrote everything very hurriedly and the performance is really bad. I am ready to deprecate all of this when the real thing came out.

# Status

- Editor : All tests passed. If you only care about development with CFS and not the deploy version, you can use this now for the time being.
- iOS : I don't have 64-bit devices with me. The latest version of Unity has a hard crash bug when built to 32-bit device.
- Android : The weakness is it is not be able to use PATCH header from `UnityWebRequest`. I have since changed PATCH (`patch` REST API) to POST (`commit` REST API) but not tested yet.
- There are tons of unprofessional `Debug.Log` left in the code currently, planned to remove once I can get everything work on iOS and Android.
- Right now I am focusing on editor-only work that wrap over Firestorm, so not going to make it work on the real device for now since Firestorm is fully usable in editor right now. I am guessing in March the official Unity SDK would come out and if that is the case then I won't lose as much time reinventing the wheel.

## This is far from identical with the real C# API

For usage of the real thing please see : https://jskeet.github.io/google-cloud-dotnet/docs/Google.Cloud.Firestore.Data/datamodel.html and you know how much you have to migrate after that thing came out.

## Why Cloud Firestore

- Cloud Firestore is described as better than Realtime Database in every way, except that it is in beta and no Unity SDK yet.
- Decision to use Realtime Database or Firestore is a big forked path, since it affects the way you would design as hierarchy with alternating collection-document (Firestore) or JSON tree design with lots of data duplications (RDB). There is probably 0% chance of easy migration. Unity devs will be faced with difficult decision of using RDB now and wait for SDK then having to overhaul design and migrate database, or just use Firestore with Firestorm while waiting for official SDK.
- The official C# Firestore API is available but Unity is not good with Nuget + it pulls in tons of dependencies that likely cause problem later. Firestorm puts all the work to `UnityWebRequest` to do REST call to ensure compatibility.

## Approach

- Use the currently available Unity Firebase SDK Auth to login before performing any Firestorm call.
- Firestorm will check on `FirebaseAuth.DefaultInstance.CurrentUser` and do `TokenAsync()`.
- The token will be an input to perform REST API call to Cloud Firestore.
- REST API performed by `UnityWebRequest`, which hopefully Unity will take care so it works with all platforms. (apparently Android could not do `UnityWebRequest` PATCH header.. whoops)
- There is nothing related to service account. I don't want to add external dependency to the Firebase Admin package.
- The Firestorm API is designed to roughly resemble C# Firestore API so that the transition to the real thing is not painful when it arrives.

## Requires

- Unity 2019.1 (**should** work with 2018.3 but I have enough time to test backward compatibility sorry..)
- Latest C#
- Firebase Unity SDK : FirebaseAuth, FirebaseApp (it must cache the `FirebaseApp` instance to prevent GC hard crash described in the mid-January patch note. If this is fixed, then only FirebaseAuth will be required.)
- Unity.Tasks that comes with Firebase Unity SDK. The Auth wants it.
- LitJSON

I put the requirement as an "assembly override" in the asmdef explicitly. It requires 3 `dll` total :
- `Firebase.App.dll`
- `Firebase.Auth.dll`
- `Unity.Tasks.dll`

For the test assembly, it requires Cloud Function also. I opt to hide Admin SDK on the server and let the cloud function handle test resets.

LitJSON is baked in the package. It is literally "little" that I could embed it in with my modifications. (60KB)

## Receiving data with LitJSON

After you got the document snapshot, `snapshot.ConvertTo<T>` will change JSON into your C# data container. How that works is according to LitJSON and may not be the same as the real upcoming SDK. Things to watch out : 

- To receive `blob` use `byte[]`.
- To receive CFS array you use `List<object>`. However, since all in this list are object LitJSON don't know which type it should convert to and boxed by `object`. For example if you have an array containing integer, double, timestamp, you will get integer as string and timestamp as string as that was what Google is sending from the server. Double is a number. Boolean is correctly a boolean. For plain fields, receiving into `DateTime` will get you `DateTime` correctly as expected as LitJson can see the type and parse the string accordingly.
- Look at LitJson test to see what else can receive what : https://github.com/LitJSON/litjson/blob/develop/test/JsonMapperTest.cs

## Why not Unity's JsonUtility

It sucks! The JSON from Firestore has polymorphic union fields (see [example](https://firebase.google.com/docs/firestore/reference/rest/v1beta1/Value)) and it is impossible to work with without good iteration method on the JSON. I used Json.NET to iterate and peel out the JSON with `JObject` LINQ support and to tailor made a JSON that Firebase would accept.


## Why not Newtonsoft Json.NET

It might be top-quality fast and reliable thanks to millions of users, but at its core it uses `DynamicMethod`. It does not work on platform like Android. If you use it, at the end you might encounter : 

```cs
  at Newtonsoft.Json.Utilities.DynamicReflectionDelegateFactory.CreateDynamicMethod (System.String name, System.Type returnType, System.Type[] parameterTypes, System.Type owner) [0x00000] in /_/Src/Newtonsoft.Json/Utilities/DynamicReflectionDelegateFactory.cs:45 

  at Newtonsoft.Json.Utilities.DynamicReflectionDelegateFactory.CreateDefaultConstructor[T] (System.Type type) [0x00000] in /_/Src/Newtonsoft.Json/Utilities/DynamicReflectionDelegateFactory.cs:244

```

And if you look at the [source](https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Utilities/DynamicReflectionDelegateFactory.cs) you can see a lot of `DynamicMethod` usage. From what I see it is trying to synthesize constructor to even a concrete type that I have everything defined beforehand. So looks like no escape.

## Limitations

I made this just enough to adopt Firestore as soon as possible. Features are at bare minimum. Code is a mess and also performance is really BAD. (Sent JSON are even indented just so that debugging would be easy..)

- Type excluded in a Document : Map inside a map (Map = dictionary of JSON not map as in world map), Geopoint (LatLng), Map for 1 level in a document is fine.
- Any mentioned types that is in an array. Basically, recursive programming is hard and I don't want to mess with it + my game does not have nested map design. But hey! Array is implemented! A friend list per player for example can be strings in an array.
- Receiving type must be a `class` with all public fields name matching names from Firestore. (It can't even be a property, see the unity test.) On getting component you must provide a concrete **type generic** with all fields known except `List<object>` which is used to receive Firestore array. It must be a `class` because it would be easy to do reflection to populate its value. It will be reflected by field name of the document to match with what's in your type. The remaining fields are left at default. You cannot substitute any fields with, for example, `Dictionary<string, string>`.
- Transaction not supported. (Used for atomic operation that rolls back together when one thing fails)
- Manual rollback not supported. (There is actually a REST endpoint for this, but too difficult to bother)
- Batched write not supported.
- Ordering not supported.
- Limiting not supported.
- Listening for realtime updates not supported.
- Query cursor/pagination not supported.
- Offline data not supported.
- Managing index not supported. (It is a long-running operation, not easy to poll for status)
- Import/export data not supported.
- No admin API supported. (Use a work around by asking Cloud Functions to do admin things including test clean up/tear down)
- Ordering of a query is locked to **ascending**. When creating a composite index please use only ascending index.
- `AddAsync` on the collection does not return the newly created document's reference but just the generated document ID.
- Exception throwing is probably not so good. But I tried to bubble up the error from Google's message from JSON REST response download handler as much as possible. (You will at least see HTTP error code)
- Only one type of sentinel value supported which is the server time. You can put `[ServerTimestamp]` attribute on any `DateTime` field that is on the **top level** of your data to write/create and it will ask the server to put a timestamp there via `DocumentTransform` REST API of `commit` command. (The top level requirement is because I am just too lazy to make it drill down and find every attributes..) See other sentinel values that are not supported, but should be in the real SDK : https://github.com/googleapis/google-cloud-dotnet/blob/master/apis/Google.Cloud.Firestore/Google.Cloud.Firestore/FieldValue.cs

## How to use

Please look in the test assembly folder for some general ideas, I don't have time to write a guide yet.. but it always begin with something like `Firestorm.Collection("c1").Document("d1").Collection("c1-1").Document("d2")._____`. (Use `FirebaseAuth.DefaultInstance` to sign in first! It works on the `CurrentUser`.)

When migrating to the real thing later, `Firestorm` would become `FirestoreDatabase` instance you get from somewhere. Everything else should be roughly the same. (?)

## How to run tests/to make sure it works

You will want to be able to pass all tests as database is a sensitive thing and could wreck your game if not careful. (Or if I made mistake somewhere)

The test will run against your **real** Firebase account and **cost real money** as it writes and cleans up the Firestore on every test (but probably not much). There are things that is required to setup beforehand.

- Do all the things that is required to make `FirebaseAuth` works in Unity. Install Unity SDK. Add `google-services.json`, `GoogleService-Info.plist` to project, etc.
- In the right click create asset menu create an asset of `FirestormConfig` and put it in `Resources` folder. Fill the form of super user information, this will be sent to Cloud Function for it to use Admin SDK to generate and destroy a test user.
- Go to your Firestore rules and add all-allowed rule for super user email like this : `allow read, write: if request.auth.token.email == "super@gmail.com";`
- Deploy a required cloud function named exactly this : `firestormTestCleanUp`. Here is the content. 

```typescript

function testSecretCheck(testSecret: string) {
    if (testSecret !== "notasecret") {
        throw new HttpsError("internal", `Your test secret ${testSecret} is incorrect.`);
    }
}

/**
 * Used for unit testing. Delete and recreate user on every test.
 * @param doNotCreateUser This is true on [TearDown] in C# so that it just delete the user and not create back. After a test there should not be any test user left.
 */
async function ensureFreshUser(email: string, password: string, doNotCreateUser: boolean) {
    try {
        const superUser: admin.auth.UserRecord = await admin.auth().getUserByEmail(email)
        //If the user exist delete him.
        await admin.auth().deleteUser(superUser.uid)
    }
    catch (e) {
        if (e.code !== "auth/user-not-found") {
            throw e
        }
        //Does not exist, it is fine.
    }
    if (doNotCreateUser === false) {
        await admin.auth().createUser({ email: email, password: password })
    }
}

export const firestormTestCleanUp = functions.https.onCall(async (data, context) => {

    const testCollectionName: string = "firestorm-test-collection"

    const testDataName1: string = "firestorm-test-data-1"
    const testDataName2: string = "firestorm-test-data-2"
    const testDataName3: string = "firestorm-test-data-3"

    const testSubCollectionName: string = "firestorm-test-sub-collection"
    const testDataName21: string = "firestorm-test-data-21"
    const testDataName22: string = "firestorm-test-data-22"

    try {
        testSecretCheck(data.testSecret)
        await Promise.all([
            ensureFreshUser(data.superUserId, data.superUserPassword, data.isTearDown),
            //No need to demolish everything, the test uses just these 5 documents.
            admin.firestore().collection(testCollectionName).doc(testDataName1).delete(),
            admin.firestore().collection(testCollectionName).doc(testDataName2).delete(),
            admin.firestore().collection(testCollectionName).doc(testDataName3).delete(),
            admin.firestore().collection(testCollectionName).doc(testDataName2).collection(testSubCollectionName).doc(testDataName21).delete(),
            admin.firestore().collection(testCollectionName).doc(testDataName2).collection(testSubCollectionName).doc(testDataName22).delete(),
        ])
    } catch (error) {
        throw new HttpsError("internal", `${error.code} -> ${error}`)
    }
});
```

Notice `testSecretCheck` method, you can change the password to match what's in your `FirestormConfig`. Every time you run each test this cloud function will run 2 times at set up and at tear down. (Costing you small amount of money)

- Put required data in the `FirestormConfig` file including super user details. This will allow us to run test without relying on including Firebase Admin API.
- Since index takes several minutes to create I cannot put it in the test without inconvenience. Go create a composite index on collection ID `firestorm-test-collection` with field `a` and `b` as both Ascending. Wait until it finishes.
- Connect to the internet and you should be able to pass all **Edit Mode** test.

### Play Mode test

Change the `asmdef` from `Editor` only to all platforms. Try and see if all the test can run successfully in play mode. It is a bit different in how it selects `FirebaseApp` instance since edit mode requires a separated instance but playmode will use `DefaultInstance` instead. (But the edit mode instance has `AppOptions` copied from `DefaultInstance` anyways)

### Real device test

The ultimate test. After making all the tests available in Play Mode, you can click the button that says **Run all in player (Android/iOS)**. The game will build now.

But in this build there are caveats :

- You will get `DEVELOPMENT_BUILD` compilation flag. If your game somehow does not build on this button click but builds on normal method, check if your code has something against this precompiler flag or not.
- Your Package name/Bundle ID will change to a fixed name : **com.UnityTestRunner.UnityTestRunner**. This will cause problem for `google-services.json` and `GoogleService-Info.plist` file as it looks to match the name and now your Firebase Unity SDK cannot initialize the Auth. (Apparently the Android ones can hold multiple package name but not iOS ones)
- To fix, please create a new set of Android/iOS app with exactly that test name in the same Firebase App (Press "Add app" button). Then download that new set of `google-services.json` and `GoogleService-Info.plist` and rename the old ones to something else because it search the whole project and pick them up by name. After the test remember to rename switch to the real one.

## "Oh no REST sucks, why don't you use gRPC?"

In short I gave up, but it looks like a better than REST way if done right. It is just too messy with Unity. (In normal C# where NUGET is usable I would do RPC way.) Also the official C# API for Firestore uses the RPC + protobuf way, so no JSON mess like what I have here. [One person even said he has successfully use gRPC from Unity](https://groups.google.com/forum/#!topic/firebase-talk/SJIgIN8hZJg), but since I have come a long way with `UnityWebRequest` I might as well continue using this as I wait. (But gRPC way will provide you with the interface most likely equal to upcoming Unity Firestore SDK, not an imitaion like Firestorm.)

### What is it

It lets you do RPC with generated code, so it feels like you are calling regular function and it magically do remote calls. The code is generated from Protobuf file. The files in GRPC folder was grabbed from generated C# files from [Google API repo](https://github.com/googleapis/googleapis).

Therefore the code to talk with Firestore in Firestorm will feel just kinda like we already have Unity SDK, because the generated gRPC codes are in C# already. (Not by doing REST to a URL, etc.)

### What is the problem

Basically the "unloading assembly because it could cause crash in runtime" error message. I add and add all requirement by Nuget chain but finally arrives at the point where I don't know what is the cause of that anymore.

### Some pointers if you want to try doing it gRPC way

First install gRPC stuff, there is a beta `unitypackage` by Google too. See [here](https://packages.grpc.io/archive/2019/01/f7a4d1e0c74f3c76bd09d8f54ab1d2c357df2788-6affcdc9-9f89-475b-817b-14263e865b8e/index.xml) for example you can see **grpc_unity_package.1.19.0-dev.zip**. Then install gRPC csharp plugin from somewhere, Google it and you should found it. It allows `protoc` to generate client stub methods when it see `service` syntax in the `.proto` file.

(At the time when you are reading this things might changed already.) Use `artman` with things in the `googleapis` repo. Go to [here](https://googleapis-artman.readthedocs.io/en/latest/installing.html) and follow it. You will be installing `pipsi` and starting `docker` daemon before you can use `artman`, then you will have to download Docker image of Google's `artman` by following the terminal. Note that all the things surrounding gRPC and `googleapis` seems to be sparsely documented than usual.

Finally you will be running something like : 

```
~/.local/bin/artman --config google/firestore/artman_firestore.yaml generate csharp_gapic
```

The `yaml` file would be updated/changed in the future? I don't know..

You will now notice that the `artman` does not include the `Firestore.Admin` section, so you cannot do gRPC with admin API. Also it is missing some more references, you will have to install more Nuget package such as [CommonProtos](https://www.nuget.org/packages/Google.Api.CommonProtos/). And in an hours or two maybe you will arrive with the same "unloading assembly" error as me?

## How about just installing Google.Cloud.Firestore and its dependencies

When I do 

```
nuget install google.cloud.firestore -Prerelease
```

I got tons of related Nuget which in turn resolves into gRPC again. I think it is scary and difficult to get it working (at runtime too) so I didn't continue this path either. Let's trust `UnityWebRequest`!!

# License

The license is MIT as you can see in `LICENSE.txt`, and to stress this is provided as-is without any warranty as said in the MIT license. (I understand that database is kind of a dangerous thing, and my programming on Firestorm is very rough as it is only a temporary solution while I am waiting for the official one.)

# Blatant advertisement

- [Introloop](http://exceed7.com/introloop/) - Easily play looping music with intro section (without physically splitting them) (Unity 2017.0+)
- [Native Audio](http://exceed7.com/native-audio/) - Lower audio latency via OS's native audio library. (Unity 2017.1+, iOS uses OpenAL / Android uses OpenSL ES)
- [Native Touch](http://exceed7.com/native-touch/) - Faster touch via callbacks from the OS, with a real hardware timestamp. (Unity 2017.1+, iOS/Android)
