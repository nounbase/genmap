# genmap
Nounbase Semantic Map Generator [for SQL Server]

> __Today, Nounbase supports only SQL Server.__ But we plan on changing that. Support for other relational databases is coming soon. Which databases should we target next? [Start a discusion and let us know!](https://github.com/nounbase/genmap/discussions)

## Semantic maps

A Nounbase semantic map is a structured representation of the relationships and meanings embedded within a relational database. It acts as a "pocket guide" for AI agents, translating the database's raw tables and fields into a comprehensive, context-rich framework that captures the essence of the domain it represents, including key entities like people, places, and things. By understanding not just the data but also the semantic connections between different elements, these maps enable AI agents to interact more intelligently and accurately with the database. This enhanced understanding allows for more precise query handling, better integration of unstructured data, and overall improved decision-making processes. For developers and data scientists, leveraging these semantic maps means their AI systems can perform more complex tasks with greater efficiency and relevance, ultimately leading to more insightful and actionable outputs.

## Prerequisites

- [Docker](https://docs.docker.com/engine/install/)
- [An OpenAI API account](https://platform.openai.com/signup) and [an OpenAI API key](https://platform.openai.com/account/api-keys)
- A source SQL Server database and its connection string
- A source schema name (e.g., dbo)

## Using `genmap`

Begin by [authenticating to the container registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry#authenticating-in-a-github-actions-workflow) and [pulling the latest `genmap` image](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry#pulling-container-images):

```shell
docker pull ghcr.io/nounbase/genmap:latest
```

Then, run the following `docker` command. Replace environment variables with your own values.

```sh
docker run -it --rm \
  -e ENV_NAME="[env_value]" \ # Refer to the "environment variables" section below
  ghcr.io/nounbase/genmap:latest
```

### Environment variables

#### `NOUNBASE_ENV_SOURCE_DB_CONNECTION_STRING` 🔴
The read-only SQL Server connection string (including basic auth/password) needed to connect to the source database

| Name | Required | Description |
| --- | --- | --- |
| `NOUNBASE_ENV_SOURCE_DB_CONNECTION_STRING` | 🔴 |  |
| `NOUNBASE_ENV_SOURCE_SCHEMA` | 🔴 | The name of the source database schema that the generated semantic map will be based on |
| `NOUNBASE_ENV_OPENAI_API_KEY` | 🔴 | Your OpenAI API key |
| `NOUNBASE_ENV_LOG` | | Set this to any value to enable detailed logging |
| `NOUNBASE_ENV_SOURCE_DB_THROTTLE` | | If provided, an integer (`1+`) indicating how many database queries can occur at the same time |
| `NOUNBASE_ENV_OPENAI_THROTTLE` | | If provided, an integer (`1+`) indicating how many OpenAI API operations can occur at the same time |
| `NOUNBASE_ENV_DEFAULT_MODEL_NAME` | | By default, the name of the OpenAI model that will be used to generate the semantic map. Default is `gpt-4o`. |
| `NOUNBASE_ENV_DISCOVERY_MODEL_NAME` | | By default, the name of the OpenAI model that will be used during the **"noun discovery phase"**. Default is `NOUNBASE_ENV_DEFAULT_MODEL_NAME`. |
| `NOUNBASE_ENV_NARRATION_MODEL_NAME` | | By default, the name of the OpenAI model that will be used during the **"narration phase"**. Default is `NOUNBASE_ENV_DEFAULT_MODEL_NAME`. |
| `NOUNBASE_ENV_ENRICHMENT_MODEL_NAME` | | By default, the name of the OpenAI model that will be used during the **"enrichment phase"**. Default is `NOUNBASE_ENV_DEFAULT_MODEL_NAME`. |

