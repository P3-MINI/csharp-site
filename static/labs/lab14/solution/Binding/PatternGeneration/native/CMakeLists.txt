cmake_minimum_required(VERSION 3.10)
project(pg C)

set(CMAKE_C_STANDARD 11)

include_directories(.)

add_library(pg SHARED export.c)

set_target_properties(pg PROPERTIES 
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}"
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}"
)
