import axios from 'axios';

const apiUrl = "http://localhost:5047/tasks"; 

export default {
  getTasks: async () => {
    const result = await axios.get(`${apiUrl}`);
    return result.data;
  },

  addTask: async (name) => {
    try {
      const response = await axios.post(`${apiUrl}`, { name });
      return response.data;
    } catch (error) {
      console.error("Error adding task:", error);
    }
  },

  setCompleted: async (id, isComplete) => {
    try {
      const response = await axios.put(`${apiUrl}/${id}`, { isComplete });
      return response.data;
    } catch (error) {
      console.error("Error updating task:", error);
    }
  },

  deleteTask: async (id) => {
    try {
      await axios.delete(`${apiUrl}/${id}`);
    } catch (error) {
      console.error("Error deleting task:", error);
    }
  },
};
