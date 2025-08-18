import React, { useEffect, useState } from "react";
import api from "../api";
import ProductForm from "./ProductForm";

export default function ProductList(){
  const [products,setProducts] = useState([]);
  const [loading,setLoading] = useState(true);
  const [error,setError] = useState(null);
  const [editing,setEditing] = useState(null);

  const fetchProducts = async () => {
    setLoading(true);setError(null);
    try {
      const res = await api.get("/products");
      setProducts(res.data);
    } catch (err) {
      setError(err?.response?.data || err.message);
    } finally { setLoading(false); }
  };

  useEffect(()=>{ fetchProducts(); },[]);

  const handleDelete = async (id) => {
    if(!confirm("Delete this product?")) return;
    try {
      await api.delete(`/products/${id}`);
      setProducts(p => p.filter(x => x.id !== id));
    } catch (err) { alert("Delete failed: "+err.message) }
  };

  return (
    <div>
      <ProductForm key={editing?.id ?? "new"} product={editing} onSaved={() => { setEditing(null); fetchProducts(); }} />
      {loading ? <p>Loading...</p>
        : error ? <p className="text-red-600">Error: {String(error)}</p>
        : (
          <table className="min-w-full mt-4">
            <thead><tr><th>Name</th><th>Price</th><th>Actions</th></tr></thead>
            <tbody>
              {products.map(p => (
                <tr key={p.id}>
                  <td>{p.name}</td>
                  <td>{p.price}</td>
                  <td>
                    <button onClick={()=>setEditing(p)}>Edit</button>
                    <button onClick={()=>handleDelete(p.id)}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )
      }
    </div>
  );
}
