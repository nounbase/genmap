# genmap
Nounbase Semantic Map Generator [for SQL Server]

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
  -e NOUNBASE_ENV_OPENAI_API_KEY="[your OpenAI API key]" \
  -e NOUNBASE_ENV_SOURCE_DB_CONNECTION_STRING="[your source SQL Server database's connection string]" \
  -e NOUNBASE_ENV_SCHEMA="[your source schema name]"
  ghcr.io/nounbase/genmap:latest
```

### Example

```shell
docker run -it --rm \
  -e NOUNBASE_ENV_OPENAI_API_KEY="sk-###" \
  -e NOUNBASE_ENV_SOURCE_DB_CONNECTION_STRING="Server=not_a_real_server..." \
  -e NOUNBASE_ENV_SCHEMA="my_schema"
  ghcr.io/nounbase/genmap:latest
```

### Example (save semantic map to `map.json`)

```shell
docker run -it --rm \
  -e NOUNBASE_ENV_OPENAI_API_KEY="sk-###" \
  -e NOUNBASE_ENV_SOURCE_DB_CONNECTION_STRING="Server=not_a_real_server..." \
  -e NOUNBASE_ENV_SCHEMA="my_schema"
  ghcr.io/nounbase/genmap:latest > map.json
```

### Example (with logging)

```shell
docker run -it --rm \
  -e NOUNBASE_ENV_OPENAI_API_KEY="sk-###" \
  -e NOUNBASE_ENV_SOURCE_DB_CONNECTION_STRING="Server=not_a_real_server..." \
  -e NOUNBASE_ENV_SCHEMA="my_schema"
  -e NOUNBASE_ENV_LOG=1
  ghcr.io/nounbase/genmap:latest
```
