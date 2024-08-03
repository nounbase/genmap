# genmap
Nounbase Semantic Map Generator [for SQL Server]

## Semantic maps

A semantic map is an enhanced representation of a database's schema, enriched with semantic annotations that detail the relationships, data types, and contextual roles within the database. This mapping provides a deeper understanding of how data elements are interconnected and their relevance in various operations, making it a valuable tool for anyone looking to improve the accuracy and relevance of database queries, especially in applications like natural language to SQL (NL2SQL) systems. Generating a semantic map can help ensure that SQL queries derived from natural language inputs are not only syntactically correct but also semantically aligned with the intended data manipulations. This leads to more effective data retrieval and management, minimizing errors and improving interaction with the database.

## Prerequisites

- [Docker](https://docs.docker.com/engine/install/)
- [An OpenAI API account](https://platform.openai.com/signup) and [an OpenAI API key](https://platform.openai.com/account/api-keys)
- A source SQL Server database and its connection string
- A source schema name (e.g., dbo)

## Using `genmap`

Begin by pulling the latest `genmap` image:

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
