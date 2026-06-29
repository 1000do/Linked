import sys

def process_copy_to_insert(input_file, output_file, tables_to_extract):
    with open(input_file, 'r', encoding='utf-8') as fin, open(output_file, 'w', encoding='utf-8') as fout:
        in_copy = False
        current_table = ""
        columns = []
        
        for line in fin:
            if not in_copy:
                if line.startswith("COPY "):
                    parts = line.strip().split(" ")
                    table_name = parts[1]
                    table_short = table_name.split('.')[-1]
                    
                    if table_short in tables_to_extract:
                        in_copy = True
                        current_table = table_name
                        # e.g., COPY public.courses (id, name, ...) FROM stdin;
                        cols_part = line[line.find("(")+1:line.find(")")]
                        columns = [c.strip() for c in cols_part.split(",")]
                        print(f"Extracting {current_table}...")
            else:
                if line.strip() == "\.":
                    in_copy = False
                    current_table = ""
                    columns = []
                    fout.write("\n")
                    continue
                
                # We are inside a COPY block for a target table
                row_data = line.strip('\n').split('\t')
                
                # Special logic: set instructor_id to 1 in courses
                if current_table == "public.courses":
                    try:
                        idx = columns.index("instructor_id")
                        row_data[idx] = "1"
                    except ValueError:
                        pass
                
                # Format values
                values = []
                for val in row_data:
                    if val == r"\N":
                        values.append("NULL")
                    else:
                        # Escape single quotes
                        val_escaped = val.replace("'", "''")
                        values.append(f"'{val_escaped}'")
                
                insert_stmt = f"INSERT INTO {current_table} ({', '.join(columns)}) VALUES ({', '.join(values)});\n"
                fout.write(insert_stmt)

if __name__ == "__main__":
    tables = [
        "courses", 
        "lessons", 
        "learning_materials", 
        "course_exts", 
        "text_embeddings", 
        "media_embeddings"
    ]
    process_copy_to_insert("db_scripts/backup_data.sql", "db_scripts/extracted_inserts.sql", tables)
    print("Done!")
