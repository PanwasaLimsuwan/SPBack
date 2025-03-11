<template>
    <div class="filter-dropdown">
      <label>{{ label }}</label>
      <select v-model="selectedValue">
        <option v-for="option in options" :key="option" :value="option">{{ option }}</option>
      </select>
    </div>
  </template>
  
  <script>
  export default {
    props: {
      label: {
        type: String,
        required: true,
      },
      options: {
        type: Array,
        required: true,
      },
      value: {
        type: String,
        required: false,
        default: "",
      },
    },
    computed: {
      selectedValue: {
        get() {
          return this.value;
        },
        set(newValue) {
          this.$emit("input", newValue);
        },
      },
    },
  };
  </script>
  
  <style scoped>
  .filter-dropdown {
    display: flex;
    flex-direction: column;
  }
  
  label {
    font-weight: bold;
    margin-bottom: 5px;
  }
  
  select {
    padding: 5px;
    border: 1px solid #ccc;
    border-radius: 4px;
  }
  </style>
  