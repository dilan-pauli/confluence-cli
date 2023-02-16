# Confluence Cloud Cli (conflutil)
Utility to explore and export data from confluence cloud.

## How to use

Download the latest release from Github or pull and build from source.

## Configuration
This CLI uses environment variables to get it's configuration. Before use please set the following environment variables.

- CONFLUENCE_URL = 'your_domain.atlassian.net'
- CONFLUENCE_USER = 'your_username'
- CONFLUENCE_APIKEY = 'your_api_key'

The API can be generated going to the [Manage your Account](https://id.atlassian.com/manage-profile/security) area when you are logged into Confluence. Then click on the Security tab and then the [Create and manage API token](https://id.atlassian.com/manage-profile/security/api-tokens) link.

To use the command in any terminal session make sure to add conflutil.exe to your PATH.

## Commands

### conflutil spaces

Returns the list of all the global, and current spaces that your account has access too. This will not include any personal spaces. By default the output is shown in a table in the console window.

Use '-c|--csv' to have the tool output the data formated in a comma separated list. The argument for this option is the path to the file that is to be created.

The columns are the key of the space, the name of the space, the status of the space, the type of the space, and a link to the space.

#### Example Output

| Key | name | status | type | Link |
|-|-|-|-|-|
| Agile | Agile Team | current | global | https://your_domain.atlassian.net/wiki/spaces/Agile 
| API | API Team | current | global | https://your_domain.atlassian.net/wiki/spaces/AED

...

### conflutil content

Returns the list of all the content that match the given CQL query.

The first positional argument to this command is the CQL query. IE, `conflutil content "lastModified < '2020/01/01' and space = API and type = page"` Will get all the pages that have a last modified time of older then 2020 and are in the API space. For more information about how 
to use the CQL see this document: https://developer.atlassian.com/cloud/confluence/advanced-searching-using-cql/

This command could take some time to execute if the result list from the query is large. It will display progress while fetching.

Use '-c|--csv' to have the tool output the data formated in a comma separated list. The argument for this option is the path to the file that is to be created.

The columns are the ID of the content, the title of the content, the status of the content, the created data, the last updated date, if the content has more then 100 char's of content to its body, the type of the content, and a link to the content.

#### Example Output

| ID | Title | status | Created Date | Last Updated | Has Content | Type | Link
|-|-|-|-|-|-|-|-|
| 134389655 | User Testing Intro | current | 2017-02-20 12:26:03  | 2017-02-23 12:26:03 | TRUE | page | https://your_domain.atlassian.net/wiki/spaces/API/pages/134389655
| 134389878 | Test Material | current | 2017-02-20 12:26:03  | 2017-02-23 12:26:03 | TRUE | page | https://your_domain.atlassian.net/wiki/spaces/API/pages/134389878

...
