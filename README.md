# genmap
Nounbase Semantic Map Generator [for SQL Server]

> __Today, Nounbase supports only SQL Server.__ But we plan on changing that. Support for other relational databases is coming soon. Which databases should we target next? [Start a discusion and let us know!](https://github.com/nounbase/genmap/discussions)

## Semantic maps

A Nounbase semantic map is a structured representation of the relationships and meanings embedded within a relational database. It acts as a "pocket guide" for AI agents, translating the database's raw tables and fields into a comprehensive, context-rich framework that captures the essence of the domain it represents, including key entities like people, places, and things. By understanding not just the data but also the semantic connections between different elements, these maps enable AI agents to interact more intelligently and accurately with the database. This enhanced understanding allows for more precise query handling, better integration of unstructured data, and overall improved decision-making processes. For developers and data scientists, leveraging these semantic maps means their AI systems can perform more complex tasks with greater efficiency and relevance, ultimately leading to more insightful and actionable outputs.

## Quickstart: Generate a semantic map

### Prerequisites

- [Docker](https://docs.docker.com/engine/install/)
- [An OpenAI API account](https://platform.openai.com/signup) and [an OpenAI API key](https://platform.openai.com/account/api-keys)
- A source SQL Server database and its connection string
- A source schema name (e.g., dbo)

### Using `genmap`

Begin by [authenticating to the container registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry#authenticating-in-a-github-actions-workflow) and [pulling the latest `genmap` image](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry#pulling-container-images):

```shell
docker pull ghcr.io/nounbase/genmap:latest
```

Then, run the following `docker` command. Replace environment variables with your own values.

```sh
docker run -it --rm \
  -e ENV_NAME="[env_value]" \ # See environment variables below
  ghcr.io/nounbase/genmap:latest
```

#### Environment variables

> 🔴 Environment variable is required.


| Name | Description |
| --- | --- |
| `NOUNBASE_ENV_SOURCE_DB_CONNECTION_STRING` | 🔴 The read-only SQL Server connection string (including basic auth/password) needed to query the source database |
| `NOUNBASE_ENV_SOURCE_SCHEMA` | 🔴 The name of the source database schema that the generated semantic map will be based on |
| `NOUNBASE_ENV_OPENAI_API_KEY` | 🔴 [Your OpenAI API key](https://platform.openai.com/account/api-keys) |
| `NOUNBASE_ENV_LOG` | Set this to any value to enable detailed logging |
| `NOUNBASE_ENV_SOURCE_DB_THROTTLE` | If provided, an integer (greater than `0`) indicating how many database queries can occur at the same time. Default is `20` database queries. |
| `NOUNBASE_ENV_OPENAI_THROTTLE` | If provided, an integer (greater than `0`) indicating how many OpenAI API operations can occur at the same time. Default is `20` API operations. |
| `NOUNBASE_ENV_DEFAULT_MODEL_NAME` | By default, the name of [the OpenAI model](https://platform.openai.com/docs/models) that will be used to generate the semantic map. Default is [`gpt-4o`](https://platform.openai.com/docs/models/gpt-4o). |
| `NOUNBASE_ENV_DISCOVERY_MODEL_NAME` | By default, the name of [the OpenAI model](https://platform.openai.com/docs/models) that will be used during the [**noun discovery phase**](#phase-3-discovery). Default is `NOUNBASE_ENV_DEFAULT_MODEL_NAME`. |
| `NOUNBASE_ENV_NARRATION_MODEL_NAME` | By default, the name of [the OpenAI model](https://platform.openai.com/docs/models) that will be used during the **"narration phase"**. Default is `NOUNBASE_ENV_DEFAULT_MODEL_NAME`. |
| `NOUNBASE_ENV_ENRICHMENT_MODEL_NAME` | By default, the name of [the OpenAI model](https://platform.openai.com/docs/models) that will be used during the **"enrichment phase"**. Default is `NOUNBASE_ENV_DEFAULT_MODEL_NAME`. |

## How **genmap** works

**genmap** constructs a layered [**Understanding**](https://github.com/nounbase/genmap/blob/main/src/Nounbase.Core/Models/Understanding.cs)) of a relational database through [a multi-phase process that transforms raw schema data into a **semantic map**](https://github.com/nounbase/genmap/blob/main/src/Nounbase.Console.GenMap/MapFactory.cs). This map represents the database’s structure, relationships, and real-world context, making it accessible to AI applications. The following breakdown explains how genmap develops this comprehensive model through schema analysis, sampling, discovery, noun sampling, narration, and enrichment, capturing both technical details and real-world meaning.

### Phase 1: Schema Analysis

- **Initial Structure**: genmap begins by analyzing [the **relational schema**](https://github.com/nounbase/genmap/blob/main/src/Nounbase.Core/Models/Schema/Schema.cs), which includes [tables](https://github.com/nounbase/genmap/blob/main/src/Nounbase.Core/Models/Schema/Table.cs), [columns](https://github.com/nounbase/genmap/blob/main/src/Nounbase.Core/Models/Schema/Column.cs), and [relationships](https://github.com/nounbase/genmap/blob/main/src/Nounbase.Core/Models/Schema/ForeignKey.cs), establishing the foundation for deeper insights into data structure and context.
- **Schema Provider Interface**: genmap is built to accommodate various databases through a [modular schema provider interface](https://github.com/nounbase/genmap/blob/main/src/Nounbase.Core/Interfaces/Factories/ISchemaFactory.cs). Currently, [it supports SQL Server](https://github.com/nounbase/genmap/blob/main/src/Nounbase.Services.SqlServer/Factories/SqlServerSchemaFactory.cs), but this design allows for easy expansion to other databases.
- **Primary and Foreign Keys**: Explicit [primary](https://github.com/nounbase/genmap/blob/77945db33efba6386b7813dbdb5ed81493cfc3b4/src/Nounbase.Core/Models/Schema/Table.cs#L22) and [foreign keys](https://github.com/nounbase/genmap/blob/main/src/Nounbase.Core/Models/Schema/ForeignKey.cs) are essential to understanding relational connections within the schema. These keys allow genmap to identify direct relationships between tables, forming a blueprint for the underlying data model.  
- **Future Enhancements**: While genmap currently requires explicitly declared foreign keys, future versions will have the capability to **automatically detect foreign key relationships** that may not be defined in the schema. By analyzing patterns in data values and structure, genmap will infer connections between tables, enabling it to map complex, unstructured schemas with minimal configuration.

### Phase 2: Sampling

- **Data Samples**: In the sampling phase, [genmap gathers **randomized samples** from each table](https://github.com/nounbase/genmap/blob/77945db33efba6386b7813dbdb5ed81493cfc3b4/src/Nounbase.Services.SqlServer/Samplers/SqlServerTableSampler.cs#L26), observing typical values within columns without processing the entire dataset. Sampling offers a representative snapshot of the data, which genmap uses to build an initial sense of context.
- **Contextual Insights**: Through these data samples, genmap can start to interpret fields that may have ambiguous or unclear names. For example, a column labeled “loc” might be identified as a location field based on sampled values, even if the name itself isn’t informative.
- **Foundation for Real-World Understanding**: Sampling serves as an anchor for later phases by providing an initial glimpse into the actual data’s characteristics and structure. This foundational understanding allows genmap to detect nuances that may not be evident from schema analysis alone, paving the way for richer interpretations in subsequent phases.

### Phase 3: Discovery

- **Core Nouns**: In the discovery phase, [genmap identifies **core noun structures** in the data](https://github.com/nounbase/genmap/blob/bf0f27404d553f233f0cea477768d2f409dbdf12/src/Nounbase.Services/Factories/NounFactory.cs#L34), such as entities representing people, places, or things. These nouns serve as the focal points around which genmap builds its deeper understanding.
- **Flattened Relationships**:
  - [**Dimension Relationships (M:1):**](https://github.com/nounbase/genmap/blob/bf0f27404d553f233f0cea477768d2f409dbdf12/src/Nounbase.Services/Factories/NounFactory.cs#L188) genmap follows dimension relationships from each core table outward, connecting tables in a way that captures the broader context of each noun. This process creates a **“snowflake” structure**, where linked tables radiate out from the core table, providing a flattened view of each entity’s characteristics and relationships.
  - [**Detail Relationships (1:M)**:](https://github.com/nounbase/genmap/blob/bf0f27404d553f233f0cea477768d2f409dbdf12/src/Nounbase.Services/Factories/NounFactory.cs#L244) genmap further explores detail relationships up to two degrees outward, capturing additional context that describes how related entities (such as orders associated with a customer) contribute to the full understanding of the noun. These multi-level connections enrich the model, adding depth to each entity’s profile.
- **Meaningful Naming**: [genmap assigns meaningful names to relationships based on conventions that align with real-world usage.] Technical fields that don’t add interpretive value, such as flags or audit timestamps, [are filtered out](https://github.com/nounbase/genmap/blob/bf0f27404d553f233f0cea477768d2f409dbdf12/src/Nounbase.Services/Factories/NounFactory.cs#L68), ensuring that the data structure remains relevant and easy to interpret.

### Phase 4: Noun Sampling

- [**Graph-Based Sampling**:](https://github.com/nounbase/genmap/blob/main/src/Nounbase.Services.SqlServer/Samplers/SqlServerNounSampler.cs) Building on the core noun definitions, genmap performs **noun sampling** across the entire noun graph, capturing the full context defined in the discovery phase.
- **Broader Context**: This expanded sampling captures insights into how data points within the noun graph are interrelated, helping genmap identify significant patterns, relationships, and groupings that reflect real-world dynamics.
- **Insight into Key Groupings**: By exploring the complete noun structure, genmap can detect which properties are suitable for segmenting data into categories, supporting more advanced analyses. This phase helps define the dimensions and metrics essential for understanding and analyzing the core objectives of the domain.

### Phase 5: Narration

- [**Descriptive Narrative**:](https://github.com/nounbase/genmap/blob/1df49bff60b33cf4205e41d608dab97c89d414b0/src/Nounbase.Services/Narrators/Narrator.cs#L33) With a fully constructed noun model, genmap generates a narrative for each noun, transforming the technical data structure into an intuitive, human-readable format.
- [**Structured Prompt**:](https://github.com/nounbase/genmap/blob/1df49bff60b33cf4205e41d608dab97c89d414b0/src/Nounbase.Services/Narrators/Narrator.cs#L145) genmap employs a structured prompt that guides AI to produce clear, relatable descriptions, capturing each noun’s purpose and relationships in conversational language.
- [**Business Relevance**:](https://github.com/nounbase/genmap/blob/1df49bff60b33cf4205e41d608dab97c89d414b0/src/Nounbase.Services/Narrators/Narrator.cs#L84) The narration phase provides context on how these entities interact and function in real-world scenarios, adding business significance to the data model. This approach helps users—both technical and non-technical—understand the data’s practical value and operational dynamics.

### Phase 6: Enrichment

The enrichment phase adds **semantic metadata** to each noun and property, enhancing the depth and usability of the noun model by refining genmap’s Understanding:

- [**Groupable Properties**:](https://github.com/nounbase/genmap/blob/22c039bddce05d28cbc2091c2000aa40106a5751/src/Nounbase.Services/Enrichers/NounEnricher.cs#L193) genmap identifies **groupable properties** that allow for meaningful data segmentation, such as `[region, country]` or `[age_group, gender]`. This supports actionable insights by enabling users to view and analyze data through high-level groupings.
- [**Real-World Naming and Classification**:](https://github.com/nounbase/genmap/blob/22c039bddce05d28cbc2091c2000aa40106a5751/src/Nounbase.Services/Enrichers/NounEnricher.cs#L348)
  - genmap assigns relatable names to each noun, making them easier to understand and use.
  - Each noun is classified as a person, place, or thing, allowing AI applications to determine its role within the model.
  - For event-based data, genmap detects chronological properties, helping the model incorporate time-based analysis and trend tracking.
- [**Detailed Property Metadata**:](https://github.com/nounbase/genmap/blob/22c039bddce05d28cbc2091c2000aa40106a5751/src/Nounbase.Services/Enrichers/NounEnricher.cs#L274) For each property, genmap gathers comprehensive metadata, enhancing the model’s usability:
  - **Description and Real-World Title**: Assigns intuitive names and descriptions, improving interpretability for non-technical users.
  - **Calculability**: Flags properties suitable for calculations, such as quantities or monetary values, supporting complex analyses.
  - **Uniqueness**: Identifies unique properties (e.g., IDs) that differentiate individual records, aiding in accurate entity resolution.
  - **Labels vs. Values**: Distinguishes categorical labels (like product category) from specific values (like product name), clarifying the role of each property.
  - **Criticality**: Highlights properties essential for strategic decision-making, ensuring that domain-critical data is prioritized.
  - **Mutability**: Labels properties as mutable (can change) or immutable (fixed), capturing dynamic and static elements within the model for accurate, long-term insights.

This detailed metadata enables genmap to capture a nuanced and real-world representation of each noun and property, providing AI applications with a powerful framework to interpret and interact with the data.

### Final Step: Creating the Semantic Map

- **JSON Format**: Once all phases are complete, genmap compiles this Understanding into a **semantic map** as a JSON document, which is portable and easy to integrate with other systems.
- **Reusable Knowledge**: This semantic map becomes a plug-and-play resource that other tools, like **describe**, can use to interpret and query the database. describe leverages this map for context-aware interactions, understanding each noun and property in real-world terms.
- **Real-World Context for AI**: By transforming relational data into an accessible, business-oriented representation, the semantic map enables AI applications to interact with the data accurately and contextually, creating a meaningful connection between technical data storage and real-world insights.

Through this structured process, genmap’s phases of schema analysis, sampling, discovery, noun sampling, narration, and enrichment work together to convert traditional relational models into a **rich, intelligent representation** of real-world data. This multi-phase pipeline enables genmap to bridge the gap between structured data storage and dynamic, business-oriented AI interactions.

## Background

The idea for Nounbase came during the rise of generative AI tools like ChatGPT, which opened up new possibilities for working with structured data. With most of the world’s data stored in relational databases, I initially set out to bridge the gap between AI and these data stores. My first attempt focused on solving natural language to SQL (NL2SQL) translation. However, I quickly realized that this approach demanded near-perfect accuracy—any misinterpretation could lead to significant errors.

To sidestep these risks, I shifted my focus to exploring how AI could "tell the story" of data within a database. The concept was to take a single row, trace its relationships, and generate a detailed narrative. While generating these articles was technically impressive, I found that it added limited value—essentially just rephrasing the data into a longer story without truly deepening its meaning. It was more a cool demonstration than a practical solution.

The real value became clear when I recognized the potential of the **semantic map** itself. By generating these maps, Nounbase could provide AI with rich context, allowing it to understand the entities and relationships within a database as if it were navigating a real-world domain. This shift in focus keeps Nounbase simple and effective: it’s all about creating semantic maps that turn structured data into something AI can truly comprehend, empowering applications to not just access data, but understand it deeply.
