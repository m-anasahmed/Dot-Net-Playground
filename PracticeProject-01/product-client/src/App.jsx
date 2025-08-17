// src/App.jsx
import { useState, useEffect } from "react";
import axios from "axios";

const API_URL = "https://localhost:7291/api/Products";

function App() {
  const [products, setProducts] = useState([]);
  const [newProduct, setNewProduct] = useState({ name: "", price: 0 });
  const [editProduct, setEditProduct] = useState(null);

  // Fetch products (GET)
  useEffect(() => {
    fetchProducts();
  }, []);

  const fetchProducts = async () => {
    try {
      const res = await axios.get(API_URL);
      setProducts(res.data);
    } catch (err) {
      console.error("Error fetching products:", err);
    }
  };

  // Add product (POST)
  const addProduct = async () => {
    try {
      await axios.post(API_URL, newProduct);
      setNewProduct({ name: "", price: 0 });
      fetchProducts();
    } catch (err) {
      console.error("Error adding product:", err);
    }
  };

  // Update product (PUT)
  const updateProduct = async (id) => {
    try {
      await axios.put(`${API_URL}/${id}`, editProduct);
      setEditProduct(null);
      fetchProducts();
    } catch (err) {
      console.error("Error updating product:", err);
    }
  };

  // Delete product (DELETE)
  const deleteProduct = async (id) => {
    try {
      await axios.delete(`${API_URL}/${id}`);
      fetchProducts();
    } catch (err) {
      console.error("Error deleting product:", err);
    }
  };

  return (
    <div style={{ margin: "20px", fontFamily: "Arial" }}>
      <h1>Product Manager</h1>

      {/* Add Product */}
      <h2>Add Product</h2>
      <input
        type="text"
        placeholder="Name"
        value={newProduct.name}
        onChange={(e) => setNewProduct({ ...newProduct, name: e.target.value })}
      />
      <input
        type="number"
        placeholder="Price"
        value={newProduct.price}
        onChange={(e) =>
          setNewProduct({ ...newProduct, price: parseFloat(e.target.value) })
        }
      />
      <button onClick={addProduct}>Add</button>

      <h2>Product List</h2>
      <ul>
        {products.map((p) => (
          <li key={p.id}>
            {editProduct?.id === p.id ? (
              <>
                <input
                  type="text"
                  value={editProduct.name}
                  onChange={(e) =>
                    setEditProduct({ ...editProduct, name: e.target.value })
                  }
                />
                <input
                  type="number"
                  value={editProduct.price}
                  onChange={(e) =>
                    setEditProduct({
                      ...editProduct,
                      price: parseFloat(e.target.value),
                    })
                  }
                />
                <button onClick={() => updateProduct(p.id)}>Save</button>
                <button onClick={() => setEditProduct(null)}>Cancel</button>
              </>
            ) : (
              <>
                {p.name} - ${p.price}
                <button onClick={() => setEditProduct(p)}>Edit</button>
                <button onClick={() => deleteProduct(p.id)}>Delete</button>
              </>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}

export default App;
