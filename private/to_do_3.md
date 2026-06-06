# LOG:
fail: Microsoft.EntityFrameworkCore.Database.Command[20102]

      Failed executing DbCommand (27ms) [Parameters=[@p0='?' (DbType = DateTime), @p1='?' (DbType = Object), @p2='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']

      INSERT INTO material_embeddings (created_at, embedding, material_id)

      VALUES (@p0, @p1, @p2)

      RETURNING embedding_id;

fail: Microsoft.EntityFrameworkCore.Update[10000]

      An exception occurred in the database while saving changes for context type 'CourseMarketplaceBE.Infrastructure.Data.AppDbContext'.

      Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.

       ---> Npgsql.PostgresException (0x80004005): 22000: expected 768 dimensions, not 512
# ISSUE:
CLIP output 512-dim vector, DistilBert output 768-dim vector, trying to stuff them to a single 768-dim vector in db cause the error

# FIXES:
## Separte material_embeddings to 2 tables
```sql
text_embeddings:
	text_embedding_id SERIAL PRIMARY KEY,
    material_id INT REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    text_embedding vector(768),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
	
media_embeddings:
	media_embedding_id SERIAL PRIMARY KEY,
    material_id INT REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    media_embedding vector(512),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
	
Reflect the update to Entities, AppDbContext, Repositories, Services

Modify MaterialEmbeddingResponse:
	In FastAPI : add field embedding_type : str ("text" or "media")
	In BE: add field string EmbeddingType ("text" or "media") 
	Ensure data between them are succeessfully transferred via redis cache
	
Cascade the update to references




	
