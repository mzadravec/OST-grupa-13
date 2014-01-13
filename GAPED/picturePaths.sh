#!/bin/bash

# echo `sqlite3 ./ost.sqlite <<< .schema`

if sqlite3 ost.sqlite <<< .schema | grep -qi 'CREATE TABLE Picture '; then
  echo "table Picture exists in ost.sqlite"
else
  sqlite3 ost.sqlite "create table Picture (id int primary key not NULL, path CHAR(50) not NULL);"
fi

for dir in ./*
do
  if [ -d $dir ]; then
    for file in $dir/*.bmp
      do
        echo "INSERTING $file, $PICTURE_ID INTO sqlite db TABLE Picture";
        sqlite3 ost.sqlite "INSERT INTO Picture (path) values (\"$file\");"
        (( PICTURE_ID++ ));
      done
  fi
done
